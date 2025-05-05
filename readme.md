Furia Know Your Fan API
API desenvolvida em .NET Core para gerenciar a integra��o com o Reddit e o armazenamento de dados de f�s.
Come�ando
Essas instru��es permitir�o que voc� configure e execute o projeto localmente para fins de desenvolvimento e teste.
Consulte Instala��o para saber como configurar o ambiente.
---
Pr�-requisitos
Certifique-se de ter os seguintes softwares instalados em sua m�quina:
�	.NET 6 SDK ou superior
�	SQL Server (ou outro banco de dados compat�vel)
�	Visual Studio 2022 (ou outro IDE compat�vel com .NET)
�	Postman (opcional, para testar os endpoints)
�	Git (para clonar o reposit�rio)
---
Instala��o
Siga os passos abaixo para configurar o ambiente de desenvolvimento:
1. Clone o reposit�rio

$ git clone <URL_DO_REPOSITORIO>
$ cd <NOME_DO_REPOSITORIO>
 
2. Configure as vari�veis de ambiente
Renomeie o arquivo .env.example para .env e configure as vari�veis de ambiente necess�rias:
$ cp .env.example .env
$ nano .env

exemplo: 
REDDIT_CLIENT_ID=<seu_client_id>
REDDIT_CLIENT_SECRET=<seu_client_secret>
REDDIT_REDIRECT_URI=http://localhost:5123/api/reddit/callback
VALIDATION_BASE_URL=http://localhost:5123
ConnectionStrings__DefaultConnection=Server=localhost;Database=FuriaKYF;User Id=sa;Password=<sua_senha>;

3. Configure o banco de dados
Certifique-se de que o SQL Server est� em execu��o e crie o banco de dados necess�rio. 
O nome do banco de dados deve corresponder ao especificado na string de conex�o (DefaultConnection).

4. Restaure as depend�ncias
No diret�rio raiz do projeto, execute o comando:
$ dotnet restore

5. Execute as migra��es do banco de dados
Aplique as migra��es para criar as tabelas no banco de dados:
$ dotnet ef database update

6. Inicie o servidor
Execute o projeto localmente:
$ dotnet run
O servidor estar� dispon�vel em: http://localhost:5123