# Modernização Alfa - COBOL + .NET

## Visão Geral

Este projeto demonstra a modernização de uma aplicação legada desenvolvida em COBOL através da utilização de uma API REST e de uma interface web desenvolvida em ASP.NET.

A arquitetura foi construída de forma que a interface nunca se comunique diretamente com o programa COBOL. Toda a comunicação ocorre através de uma camada intermediária (**zosproxy**), responsável por traduzir requisições HTTP em registros de tamanho fixo compreendidos pelo sistema legado.

Esta abordagem simula a arquitetura utilizada em ambientes IBM com **z/OS Connect**, permitindo que aplicações modernas consumam funcionalidades escritas em COBOL através de APIs REST.

---

# Arquitetura

```
┌───────────────────────────┐
│       Navegador Web       │
└─────────────┬─────────────┘
              │ HTTP
              ▼
┌───────────────────────────┐
│      WebInterface         │
│ ASP.NET Razor Pages (.NET)│
└─────────────┬─────────────┘
              │ HTTP / JSON
              ▼
┌───────────────────────────┐
│        zosproxy           │
│      API REST (.NET)      │
└─────────────┬─────────────┘
              │
              │ Registro de tamanho fixo
              ▼
┌───────────────────────────┐
│      Legacy COBOL         │
│        ALFA.cbl           │
└─────────────┬─────────────┘
              │
              ▼
        clientes.dat
```

---

# Estrutura do Projeto

```
Modernizacao_Alfa_COBOL/

├── LegacyCobol/
│   ├── ALFA.cbl
│   ├── alfa
│   ├── data/
│   │   └── clientes.dat
│   └── io/
│       ├── request.txt
│       └── response.txt
│
├── zosproxy/
│   ├── Controllers/
│   ├── DTO/
│   ├── Services/
│   └── Program.cs
│
├── WebInterface/
│   ├── DTO/
│   ├── Pages/
│   ├── Service/
│   └── Program.cs
│
└── tests/
    └── zosproxy.IntegrationTests/
```

---

# Componentes

## Legacy COBOL

O programa COBOL concentra toda a regra de negócio da aplicação.

Responsabilidades:

- Ler o arquivo `request.txt`
- Interpretar a operação solicitada
- Consultar os dados dos clientes
- Atualizar informações de contato
- Persistir alterações
- Gerar o arquivo `response.txt`

Operações implementadas:

- CONSULTAR
- ATUALIZAR

Toda comunicação ocorre através de registros de tamanho fixo.

---

## Formato da Requisição

```
+------------+------+-----------+----------------------------------------+
| Operação   | ID   | Telefone  | E-mail                                 |
+------------+------+-----------+----------------------------------------+
| X(10)      | X(6) | X(11)     | X(80)                                  |
+------------+------+-----------+----------------------------------------+
```

Exemplo:

```
CONSULTAR000001
```

ou

```
ATUALIZAR00000162999990000novo@email.com
```

---

## Formato da Resposta

O COBOL retorna um registro separado por `"|"`.

```
CodigoRetorno|Mensagem|ID|Nome|Telefone|Email
```

Exemplo:

```
0000|Cliente encontrado.|000001|Maria Silva|62999990000|maria.silva@email.com
```

---

# zosproxy

O **zosproxy** atua como intermediário entre aplicações modernas e o sistema legado.

Responsabilidades:

- Receber requisições HTTP
- Validar os dados recebidos
- Gerar o arquivo `request.txt`
- Executar o programa COBOL
- Ler o arquivo `response.txt`
- Converter a resposta em JSON
- Retornar a resposta HTTP apropriada

Toda a complexidade de comunicação com o COBOL fica encapsulada nesta camada.

---

# LocalCobolGateway

A classe `LocalCobolGateway` concentra toda a integração com o sistema legado.

Responsabilidades:

- Criar o `request.txt`
- Executar o programa COBOL
- Aguardar sua execução
- Ler o `response.txt`
- Converter o retorno em objetos C#

É utilizado um `SemaphoreSlim` para impedir que duas requisições utilizem simultaneamente os mesmos arquivos de comunicação.

---

# WebInterface

A interface Web foi desenvolvida utilizando ASP.NET Razor Pages.

Responsabilidades:

- Receber os dados do usuário
- Consumir a API REST
- Exibir os resultados da consulta
- Permitir a atualização dos dados de contato

A interface nunca acessa diretamente o programa COBOL.

---

# Endpoints

## Consultar Cliente

```
GET

/api/clientes/{id}
```

Exemplo:

```
GET /api/clientes/000001
```

Resposta:

```json
{
  "success": true,
  "responseCode": "0000",
  "message": "Cliente encontrado.",
  "client": {
    "id": "000001",
    "name": "Maria Silva",
    "email": "maria.silva@email.com",
    "number": "62999990000"
  }
}
```

---

## Atualizar Contato

```
PUT

/api/clientes/{id}/contato
```

Body:

```json
{
  "email": "novo@email.com",
  "number": "62999990000"
}
```

---

# Execução do Projeto

## 1. Compilar o COBOL

```bash
cd LegacyCobol

cobc -x -free -o alfa ALFA.cbl
```

---

## 2. Executar o zosproxy

```bash
dotnet run --project zosproxy
```

Padrão:

```
http://localhost:5005
```

---

## 3. Executar a Interface Web

```bash
dotnet run --project WebInterface
```

Acesse o endereço exibido pelo ASP.NET no navegador.

---

# Testando a API

Consultar cliente:

```bash
curl http://localhost:5005/api/clientes/000001
```

Atualizar contato:

```bash
curl -X PUT http://localhost:5005/api/clientes/000001/contato \
-H "Content-Type: application/json" \
-d '{"email":"novo@email.com","number":"62999990000"}'
```

---

# Testes de Integração

Os testes foram desenvolvidos utilizando **xUnit**.

Execução:

```bash
dotnet test tests/zosproxy.IntegrationTests/zosproxy.IntegrationTests.csproj
```

Os testes validam:

- Consulta de cliente existente
- Consulta de cliente inexistente
- Atualização de telefone e e-mail
- Persistência dos dados após atualização

Os testes percorrem todo o fluxo da aplicação:

```
xUnit

↓

HTTP

↓

zosproxy

↓

COBOL

↓

clientes.dat
```

---

# Armazenamento Atual

Os clientes são armazenados em:

```
LegacyCobol/data/clientes.dat
```

Formato:

```
ID|Nome|Telefone|Email
```

Exemplo:

```
000001|Maria Silva|62999990000|maria.silva@email.com
```

---

# Objetivos Educacionais

Este projeto demonstra conceitos importantes utilizados na integração entre sistemas legados e aplicações modernas:

- Modernização de sistemas legados
- Integração entre COBOL e .NET
- Desenvolvimento de APIs REST
- Comunicação entre camadas utilizando DTOs
- Arquitetura em camadas
- Separação de responsabilidades
- Testes de integração com xUnit
- Comunicação utilizando registros de tamanho fixo
- Desenvolvimento de aplicações modernas em ASP.NET

A arquitetura implementada é semelhante à utilizada em ambientes corporativos que integram aplicações modernas com sistemas IBM Mainframe.
