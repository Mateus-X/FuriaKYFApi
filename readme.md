Furia Know Your Fan API
API desenvolvida em .NET Core para gerenciar a integração com o Reddit e o armazenamento de dados de fãs.
Começando
Essas instruções permitirão que você configure e execute o projeto localmente para fins de desenvolvimento e teste.
Consulte Instalação para saber como configurar o ambiente.
---
Pré-requisitos
Certifique-se de ter os seguintes softwares instalados em sua máquina:
•	.NET 6 SDK ou superior
•	SQL Server (ou outro banco de dados compatível)
•	Visual Studio 2022 (ou outro IDE compatível com .NET)
•	Postman (opcional, para testar os endpoints)
•	Git (para clonar o repositório)
---
Instalação
Siga os passos abaixo para configurar o ambiente de desenvolvimento:
1. Clone o repositório

$ git clone <URL_DO_REPOSITORIO>
$ cd <NOME_DO_REPOSITORIO>
 
2. Configure as variáveis de ambiente
Renomeie o arquivo .env.example para .env e configure as variáveis de ambiente necessárias:
$ cp .env.example .env
$ nano .env

exemplo: 
REDDIT_CLIENT_ID=<seu_client_id>
REDDIT_CLIENT_SECRET=<seu_client_secret>
REDDIT_REDIRECT_URI=http://localhost:5123/api/reddit/callback
VALIDATION_BASE_URL=http://localhost:5123
ConnectionStrings__DefaultConnection=Server=localhost;Database=FuriaKYF;User Id=sa;Password=<sua_senha>;

3. Configure o banco de dados
Certifique-se de que o SQL Server está em execução e crie o banco de dados necessário. 
O nome do banco de dados deve corresponder ao especificado na string de conexão (DefaultConnection).

4. Restaure as dependências
No diretório raiz do projeto, execute o comando:
$ dotnet restore

5. Execute as migrações do banco de dados
Aplique as migrações para criar as tabelas no banco de dados:
$ dotnet ef database update

6. Inicie o servidor
Execute o projeto localmente:
$ dotnet run
O servidor estará disponível em: http://localhost:5123