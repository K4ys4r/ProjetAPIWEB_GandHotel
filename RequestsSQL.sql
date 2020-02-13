RESTORE DATABASE GrandHotel FROM DISK = 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.bak' WITH REPLACE,
MOVE 'GrandHotel' TO 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.mdf',
MOVE 'GrandHotel_log' TO 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.ldf'

--1.	Les clients pour lesquels on n’a pas de numéro de portable (id, nom) 
-- 48 lignes

select c.Id, c.Nom, c.Prenom from Client c
inner join Telephone t on t.IdClient= c.Id
where t.Numero like ('06%') or t.Numero like ('07%')


--2.	Le taux moyen de réservation de l’hôtel par mois-année (2015-01, 2015-02…), c'est à dire la moyenne sur les chambres du ratio (nombre de jours de réservation dans le mois / nombre de jours du mois)
--47 lignes

select cha.Numero, format(r.jour,'yyyy-MM') Dates,(count(distinct(r.Jour))/count(day(eomonth(datefromParts(Year(cal.jour),Month(cal.jour),1))))) TauxReserv from Chambre cha
inner join Reservation r on cha.Numero = r.NumChambre
inner join Calendrier cal on cal.Jour = r.Jour
group by cha.Numero, format(r.jour,'yyyy-MM')

select count (jour) from calendrier
select count (distinct jour) from Reservation
select day(eomonth(datefromParts(Year(jour),Month(jour),1))) from calendrier

select Numero,format(r.Jour,'yyyy-MM') Dates, (convert(decimal,count(distinct r.Jour))/day(eomonth(datefromParts(Year(cal.jour),Month(cal.jour),1)))) from Chambre cha
join Reservation r on r.NumChambre=cha.Numero
inner join Calendrier cal on cal.Jour = r.Jour
group by Numero,format(r.Jour,'yyyy-MM'), datefromParts(Year(cal.jour),Month(cal.jour),1)


--3.	Le nombre total de jours réservés par les clients ayant une carte de fidélité au cours de la dernière année du calendrier (obtenue dynamiquement)
-- 42 jours de réservation en 2018

select top(1) year(cal.jour) ,count(distinct r.jour) from Reservation r
inner join Calendrier cal on cal.Jour = r.Jour
inner join Client c on c.Id = IdClient
where c.CarteFidelite=1
group by year(cal.Jour)
order by year(cal.Jour) desc


--4.	Le chiffre d’affaire de l’hôtel par trimestre de chaque année



--5.	Le nombre de clients dans chaque tranche de 1000 € de chiffre d’affaire total généré. La première tranche est < 5000 €, et la dernière >= 8000 €


--6.	Code T-SQL pour augmenter à partir du 01/01/2019 les tarifs des chambres de type 1 de 5%, et ceux des chambres de type 2 de 4% par rapport à l'année précédente


--7.	Clients qui ont passé au total au moins 7 jours à l’hôtel au cours d’un même mois (Id, Nom, mois où ils ont passé au moins 7 jours)


--8.	Clients qui sont restés à l’hôtel au moins deux jours de suite au cours de l’année 2017
