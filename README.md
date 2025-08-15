# Bacon Games Challenge

Este documento proporciona instrucciones claras para configurar y ejecutar el proyecto Challenge, así como los tests asociados en Challenge.Test.

## Requisitos previos
.NET SDK (versión compatible con el proyecto : .Net9)

IDE de preferencia (Visual Studio, VS Code, Rider, etc.)

Git (opcional, si se clona el repositorio)

git clone https://github.com/RockStoneStudios/bacon_games.git


### Restaurar dependencias:

en consola
dotnet restore





## Iniciar la aplicación:

desde el root : dotnet run --project Challenge 
### o
desde la carpeta challenge dotnet run 


La aplicación debería iniciarse en https://localhost:5001 o http://localhost:5000  y lo abres con swagger  agregando /swagger ejemplo : http://localhost:5000/swagger

## Credenciales de Acceso

Email: omar@gmail.com
Contraseña: 12345678
una ves logueado el token agregalo al authorized como Bearer eyak40a9os....p68s5

<img width="1297" height="596" alt="image" src="https://github.com/user-attachments/assets/ef61a7dd-c9f5-4d2b-b114-267da0bfcc7f" />

<img width="1269" height="453" alt="image" src="https://github.com/user-attachments/assets/a41d0b65-7385-4ce9-8b2d-c258cad61b6e" />



## Ejecución de tests
Para ejecutar los tests unitarios:

desde el root ejecuta 
dotnet test 

(Los endpoints de buscar pokemon por id y por nombre no estan protegidos los protegidos son el de agregar un nuevo pokemon y el inventario como pedia el desafio)









