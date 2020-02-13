RESTORE DATABASE GrandHotel FROM DISK = 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.bak' WITH REPLACE,
MOVE 'GrandHotel' TO 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.mdf',
MOVE 'GrandHotel_log' TO 'C:\Users\Adminl\Desktop\Projet Web API\GrandHotel.ldf'