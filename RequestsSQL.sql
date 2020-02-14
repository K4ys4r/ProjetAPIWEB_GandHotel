RESTORE DATABASE GrandHotel FROM DISK = 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.bak' WITH REPLACE,
MOVE 'GrandHotel' TO 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.mdf',
MOVE 'GrandHotel_log' TO 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.ldf'

--1.	Les clients pour lesquels on n’a pas de numéro de portable (id, nom) 
-- 553 lignes

select c.Id, c.Nom, c.Prenom 
from Client c
inner join Telephone t on t.IdClient= c.Id
where t.CodeType = 'F'

except

select c.Id, c.Nom, c.Prenom from Client c
inner join Telephone t on t.IdClient= c.Id
where t.CodeType='M'


--2.	Le taux moyen de réservation de l’hôtel par mois-année (2015-01, 2015-02…), c'est à dire la moyenne sur les chambres du ratio (nombre de jours de réservation dans le mois / nombre de jours du mois)
--47 lignes

select R.Dates, avg(R.Taux_Reservation_Chambre) Taux_Reservation_Hotel --Moyenne du taux de réservation de toutes les chambres par mois
from
	(
	select Numero,format(r.Jour,'yyyy-MM') Dates, 
-- Calcul du taux de réservation pour chaque chambre par mois
	(convert(decimal,count(distinct r.Jour))/day(eomonth(datefromParts(Year(cal.jour),Month(cal.jour),1)))) Taux_Reservation_Chambre
	from Chambre cha
	inner join Reservation r on r.NumChambre=cha.Numero
	inner join Calendrier cal on cal.Jour = r.Jour
	group by Numero,format(r.Jour,'yyyy-MM'), datefromParts(Year(cal.jour),Month(cal.jour),1)
	) as R 
group by R.dates


--3.	Le nombre total de jours réservés par les clients ayant une carte de fidélité au cours de la dernière année du calendrier (obtenue dynamiquement)
-- 42 jours de réservation en 2018

select top(1) year(cal.jour) Année ,count(distinct r.jour)Jours_Réservés_Fidélité 
from Reservation r
inner join Calendrier cal on cal.Jour = r.Jour
inner join Client c on c.Id = IdClient
where c.CarteFidelite=1
group by year(cal.Jour)
order by year(cal.Jour) desc


--4.	Le chiffre d’affaire de l’hôtel par trimestre de chaque année
--Annee Trimestre CA
--2016	T1	50955.25
--2016	T2	84953.00
--2016	T3	84634.00
--2016	T4	92696.34
--2017	T1	98100.33
--2017	T2	103196.72
--2017	T3	102002.12
--2017	T4	120939.50
--2018	T1	42281.80

select R.Annee, R.Trimestre, convert(decimal(10,2),sum(R.CA_Mois)) CA_Trimestre 
from
(
	select  year(f.DatePaiement) Annee,
	case -- Définition des trimestres
		when month(f.DatePaiement) between 1 and 3  then 'T1'
		when month(f.DatePaiement) between 4 and 6  then 'T2'
		when month(f.DatePaiement) between 7 and 9  then 'T3'
		when month(f.DatePaiement) between 9 and 12  then 'T4'
	end as Trimestre,
	-- Calcul des CA regroupés par mois
	sum((lf.MontantHT*(1-lf.TauxReduction)*(1+lf.TauxTVA)*lf.Quantite)) CA_Mois
	from Facture f
	inner join LigneFacture lf on f.Id=lf.IdFacture
	group by year(f.DatePaiement),month(f.DatePaiement)
) as R
group by R.Annee,R.Trimestre
order by R.Annee,R.Trimestre

--5.	Le nombre de clients dans chaque tranche de 1000 € de chiffre d’affaire total généré. La première tranche est < 5000 €, et la dernière >= 8000 €


select R.Tranche_CA, count(R.IdClient) as NbClients -- Regroupement par tranche pour compter le nombre de clients
from
	(
	--Attribution des tranches aux clients
	select IdClient, sum((lf.MontantHT*(1-lf.TauxReduction)*(1+lf.TauxTVA)*lf.Quantite)) CA_Client, 
	case --Création des différents tranches CA
	when sum((lf.MontantHT*(1-lf.TauxReduction)*(1+lf.TauxTVA)*lf.Quantite)) < 5000  then 'CA < 5000'
	when sum((lf.MontantHT*(1-lf.TauxReduction)*(1+lf.TauxTVA)*lf.Quantite)) between 5000 and 6000  then '5000 >= CA > 6000'
	when sum((lf.MontantHT*(1-lf.TauxReduction)*(1+lf.TauxTVA)*lf.Quantite)) between 6000 and 7000  then '6000 >= CA > 7000'
	when sum((lf.MontantHT*(1-lf.TauxReduction)*(1+lf.TauxTVA)*lf.Quantite)) between 7000 and 8000  then '7000 >= CA > 8000'
	when sum((lf.MontantHT*(1-lf.TauxReduction)*(1+lf.TauxTVA)*lf.Quantite)) >=8000  then 'CA >= 8000'
	end as Tranche_CA
	from Facture f
	inner join LigneFacture lf on f.Id=lf.IdFacture
	inner join Client c on c.Id= f.IdClient
	group by IdClient
	) as R
group by R.Tranche_CA

--6.	Code T-SQL pour augmenter à partir du 01/01/2019 les tarifs des chambres de type 1 de 5%, et ceux des chambres de type 2 de 4% par rapport à l'année précédente

drop procedure if exists EvolutionPrix 
go

create procedure EvolutionPrix  
as
begin

declare @taux as decimal (10,2)
declare @prix as decimal (10,2)
declare @date as date

-- Insertion dans les tables Tarif et TarifChambre pour les chambres de type 1
set @taux = 0.05
set @prix =(select top(1)Prix*(1.0+@taux) from Tarif where Code like ('CHB1%') order by DateDebut desc) 
set @date = dateadd(year,1,(select top(1) DateDebut from Tarif where Code like ('CHB1%') order by DateDebut desc)) 
insert Tarif (Code,DateDebut,Prix) values (('CHB1-'+convert(nvarchar,year(@date))),(@date),(@prix))
insert TarifChambre select distinct(NumChambre),
('CHB1-'+convert(nvarchar,year(@date))) from TarifChambre where CodeTarif like ('CHB1%')

-- Insertion dans les tables Tarif et TarifChambre pour les chambres de type 2
set @taux = 0.04
set @prix =(select top(1)Prix*(1.0+@taux) from Tarif where Code like ('CHB2%') order by DateDebut desc) 
set @date = dateadd(year,1,(select top(1) DateDebut from Tarif where Code like ('CHB2%') order by DateDebut desc)) 
insert Tarif (Code,DateDebut,Prix) values (('CHB2-'+convert(nvarchar,year(@date))),(@date),(@prix))
insert TarifChambre select distinct(NumChambre),
('CHB2-'+convert(nvarchar,year(@date))) from TarifChambre where CodeTarif like ('CHB2%')

end 
go

begin tran
exec EvolutionPrix

rollback tran

--7.	Clients qui ont passé au total au moins 7 jours à l’hôtel au cours d’un même mois (Id, Nom, mois où ils ont passé au moins 7 jours)
-- Stimac et Milionis

select Id, Nom, format(r.Jour,'yyyy-MM') Mois from Client c
inner join Reservation r on r.IdClient=c.Id
group by format(r.Jour,'yyyy-MM'), Id, Nom
having count(r.Jour)>=7

--8.	Clients qui sont restés à l’hôtel au moins deux jours de suite au cours de l’année 2017
--11 & 277 & 505
-- On compare 2 réservations du même client et on vérifie qu'il n'y a qu'une journée de différence
select distinct(r.IdClient) from Reservation r
where year(r.Jour) = 2017 and 1 in
(select datediff(day,r.Jour,r2.Jour) from Reservation r2
where year(r.Jour) = 2017 and r.IdClient = r2.IdClient)
