# Desafio: Importação de Clientes via CSV

## Visão geral
Este repositório contém uma solução **.NET 9** destinada a importar registros de clientes a partir de arquivos CSV. O upload é feito via API, que armazena o arquivo no S3 e envia uma mensagem para uma fila SQS. Um worker em segundo plano consome as mensagens, processa o arquivo e grava os dados válidos no banco de dados SQL Server. Logs e métricas são enviados ao CloudWatch.

## Arquitetura e fluxo
A aplicação utiliza os serviços AWS no seguinte fluxo:

<img width="912" height="372" alt="image" src="https://github.com/user-attachments/assets/35f70c25-ae10-4f98-9b99-bee7a1ecaaa8" />

```
Cliente → API → Amazon S3 → Amazon SQS → CsvQueueWorker → SQL Server
                                    ↘︎ Dead Letter Queue
                                     ↘︎ CloudWatch (logs/métricas)
```

- **Armazenamento dos arquivos:** o handler de importação delega a `CsvProcessorService.UploadToS3AndEnqueueAsync`, que envia o CSV para o Amazon S3 e publica uma mensagem com a chave do arquivo no SQS.
- **Processamento assíncrono:** o worker `CsvQueueWorker` lê a fila SQS, processa mensagens em paralelo e delega a `CsvProcessorService.ProcessAsync` a leitura e persistência dos registros.
- **Resiliência:** registros inválidos ou com erro são enviados para uma dead-letter queue e métricas/alarms são criadas via CloudWatch para falhas e backlog da fila.

## Estrutura do código .NET
- **DOMAIN:** entidades e interfaces de repositório (e.g., `Client` e `IClientRepository`).
- **APPLICATION:** handlers MediatR e interfaces de serviços, responsáveis pela orquestração de casos de uso (ex.: `ImportCsvCommandHandler`).
- **INFRASTRUCTURE:** implementação de repositórios, serviços AWS e worker de fila.
- **INFRASTRUCTURE.IOC:** implementação de repositórios, serviços AWS e worker de fila; configurado via `AddInfrastructure` em `Program.cs`.
- **API:** camada de apresentação, expondo endpoints REST e utilizando MediatR para intermediar as requisições.
- **COMMON:** DTOs, mapeamentos e validadores reutilizáveis.

### Validação escalável de dados do CSV
Validação de CPF e e-mail foram isoladas em utilitários (`CpfValidator`, `EmailValidator`), usados pelo `CsvProcessorService` durante a leitura em fluxo, permitindo reuso em qualquer comando ou serviço.

## Requisitos
- .NET SDK 9.0
- Banco de dados SQL Server acessível
- Conta AWS com permissões para S3, SQS e CloudWatch
- Credenciais AWS configuradas localmente (`~/.aws/credentials` ou variáveis `AWS_ACCESS_KEY_ID`/`AWS_SECRET_ACCESS_KEY`)

## Configuração
1. Clone o repositório:
   ```bash
   git clone https://github.com/Anderthonn/PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.git
   cd PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE
   ```
2. Edite `PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.API/appsettings.json` ou defina variáveis de ambiente com os valores:
   - `ConnectionStrings:DefaultConnection` – string de conexão do SQL Server
   - `AWS:Profile` e `AWS:Region` – perfil e região da AWS
   - `Aws:BucketName` – bucket S3 para armazenar os CSVs
   - `Aws:QueueUrl` – URL da fila SQS que dispara o processamento
   - `Aws:DeadLetterQueueUrl` – URL da Dead Letter Queue (opcional)
   - `CsvQueueWorker:DegreeOfParallelism` – grau de paralelismo do worker

   Exemplo:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=CsvImportDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
     },
     "AWS": {
       "Profile": "default",
       "Region": "us-east-1"
     },
     "Aws": {
       "BucketName": "csv-import-bucket",
       "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789012/import-queue",
       "DeadLetterQueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789012/import-dlq",
       "MaxRetries": 5
     },
     "CsvQueueWorker": {
       "DegreeOfParallelism": 5
     }
   }
   ```

## Execução
1. Restaurar dependências e compilar:
   ```bash
   dotnet restore
   dotnet build
   ```
2. Garanta que o banco de dados exista (crie e aplique migrations, se necessário).
3. Execute a API (o worker é registrado como Hosted Service):
   ```bash
   dotnet run --project PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.API
   ```
   A documentação via Swagger estará disponível em ambiente de desenvolvimento em `https://localhost:5001/swagger` (ou porta configurada).

## Endpoints principais
- `POST /api/clientes/importar` – envia um arquivo CSV (multipart/form-data) e retorna `RequestId`.
- `POST /api/clientes/register` – registra cliente manualmente.
- `PUT /api/clientes/update` – atualiza um cliente.
- `DELETE /api/clientes/delete?id=1` – remove um cliente.
- `GET /api/clientes/get-all` – lista todos os clientes.
- `GET /api/clientes/get-by-id?id=1` – busca cliente pelo identificador.

Formato esperado do CSV:
```
Name,Cpf,Email
Joao,12345678901,joao@example.com
```

## Testes
Execute os testes unitários:
```bash
dotnet test
```

## Monitoramento
Logs e métricas: `CloudWatchMonitoringService` centraliza gravação de logs (CloudWatch Logs) e publicação de métricas personalizadas (CloudWatch Metrics) para todo o pipeline.
Alarmes e indicadores: o mesmo serviço cria alarmes de falha, backlog de fila e exceções críticas, permitindo alertas automáticos e análise de desempenho do processamento.

## Observação
O conteúdo do README foi estruturado para responder às perguntas presentes no documento do desafio.
Segue abaixo para maior esclarecimento o resumo das respostas.

1. Desenhe ou descreva a arquitetura da solução utilizando serviços da AWS.
   - Quais serviços AWS você utilizaria para armazenar os arquivos?
     <img width="912" height="372" alt="image" src="https://github.com/user-attachments/assets/e583784b-47d6-4c19-a122-945902274785" />
     
     ```
      Cliente → API → Amazon S3 → Amazon SQS → CsvQueueWorker → SQL Server
                                      ↘︎ Dead Letter Queue
                                       ↘︎ CloudWatch (logs/métricas)
     ```
     Armazenamento dos arquivos: o handler de importação delega a `CsvProcessorService.UploadToS3AndEnqueueAsync`, que envia o CSV para o Amazon S3 e publica uma mensagem com a chave do arquivo no SQS.
   - Como você implementaria o fluxo assíncrono de processamento?  
     O worker `CsvQueueWorker` lê a fila SQS, processa mensagens em paralelo e delega a `CsvProcessorService.ProcessAsync` a leitura e persistência dos registros.
   - Como garantiria resiliência em caso de falhas durante o processamento?  
     Registros inválidos ou com erro são enviados para uma dead-letter queue e métricas/alarms são criadas via CloudWatch para falhas e backlog da fila.

2. Explique como seria a estrutura do código .NET:
   - Como organizaria os projetos e camadas (API, serviços, domínio, infraestrutura)?  
     DOMAIN: entidades e interfaces de repositório (e.g., `Client` e `IClientRepository`).  
     APPLICATION: handlers MediatR e interfaces de serviços, responsáveis pela orquestração de casos de uso (ex.: `ImportCsvCommandHandler`).  
     INFRASTRUCTURE: implementação de repositórios, serviços AWS e worker de fila; configurado via `AddInfrastructure` em `Program.cs`.  
     API: camada de apresentação, expondo endpoints REST e utilizando MediatR para intermediar as requisições.  
     COMMON: DTOs, mapeamentos e validadores reutilizáveis.
   - Como faria a validação dos dados do CSV de forma escalável e reutilizável?  
     Validação de CPF e e-mail foram isoladas em utilitários (`CpfValidator`, `EmailValidator`), usados pelo `CsvProcessorService` durante a leitura em fluxo, permitindo reuso em qualquer comando ou serviço.

3. Explique como você monitora esse fluxo na AWS.  
   Logs e métricas: `CloudWatchMonitoringService` centraliza gravação de logs (CloudWatch Logs) e publicação de métricas personalizadas (CloudWatch Metrics) para todo o pipeline.  
   Alarmes e indicadores: o mesmo serviço cria alarmes de falha, backlog de fila e exceções críticas, permitindo alertas automáticos e análise de desempenho do processamento.


    
