# Modernização Alfa - COBOL + .NET

## Visão geral

Este projeto implementa uma solução de modernização para um sistema legado da Cooperativa Financeira Alfa.

O objetivo é permitir que um atendente consulte e atualize informações de clientes por meio de uma interface web simples, mantendo o processamento principal em um componente COBOL.

A solução utiliza:

- **ASP.NET Razor Pages** para a interface web;
- **ASP.NET Web API** como proxy local, chamado `zosproxy`;
- **COBOL** para o processamento legado;
- **xUnit** para testes automatizados de integração.

A arquitetura simula, em ambiente local, o papel que o **z/OS Connect** teria em uma arquitetura real de mainframe: receber requisições HTTP/JSON, converter os dados para uma estrutura compreendida pelo COBOL, executar o processamento legado e retornar uma resposta moderna em JSON.

---

## Arquitetura da solução

```text
Navegador
   -> WebInterface
      -> ClientCobolApi
         -> HTTP/JSON
            -> zosproxy
               -> LocalCobolGateway
                  -> request.txt
                     -> ALFA.cbl / alfa
                        -> clientes.dat
                     -> response.txt
                  -> JSON
```

Fluxo resumido:

1. O usuário acessa a interface web.
2. A interface envia uma requisição HTTP para o `zosproxy`.
3. O `zosproxy` monta um registro de tamanho fixo no arquivo `request.txt`.
4. O `zosproxy` executa o programa COBOL compilado.
5. O COBOL lê a requisição, processa a operação e grava `response.txt`.
6. O `zosproxy` lê a resposta do COBOL e converte para JSON.
7. A interface web exibe o resultado para o usuário.

---

## Estrutura do projeto

```text
Modernizacao_Alfa_COBOL/

├── LegacyCobol/
│   ├── ALFA.cbl
│   ├── data/
│   │   └── clientes.dat
│   └── io/
│       ├── request.txt
│       └── response.txt
│
├── zosproxy/
│   ├── Controllers/
│   ├── DTO/
│   ├── Service/
│   ├── Program.cs
│   └── zosproxy.csproj
│
├── WebInterface/
│   ├── DTO/
│   ├── Pages/
│   ├── Service/
│   ├── Program.cs
│   └── WebInterface.csproj
│
├── tests/
│   └── zosproxy.IntegrationTests/
│
└── ModernizingAlfa.slnx
```

---

## Componentes principais

### 1. WebInterface

Projeto ASP.NET Razor Pages responsável pela interface com o usuário.

Funções principais:

- Exibir formulário para consulta de cliente;
- Exibir formulário para atualização de telefone e e-mail;
- Enviar requisições para o `zosproxy`;
- Exibir os dados retornados pela API.

A interface web não conhece os detalhes de execução do COBOL. Ela se comunica apenas com o `zosproxy` via HTTP/JSON.

---

### 2. zosproxy

Projeto ASP.NET Web API que atua como camada intermediária entre a aplicação moderna e o componente legado.

Funções principais:

- Receber chamadas REST;
- Validar dados de entrada;
- Converter JSON para o formato esperado pelo COBOL;
- Executar o programa COBOL;
- Ler a resposta gerada pelo COBOL;
- Converter o retorno para JSON;
- Retornar o status HTTP adequado.

Essa camada representa uma simulação local do papel de um provedor de API no estilo z/OS Connect.

---

### 3. LocalCobolGateway

Classe responsável por encapsular a comunicação com o programa COBOL.

Funções principais:

- Criar o arquivo `LegacyCobol/io/request.txt`;
- Executar o binário COBOL `alfa`;
- Aguardar a finalização do processo;
- Ler o arquivo `LegacyCobol/io/response.txt`;
- Converter a resposta do COBOL para DTOs C#.

Como a comunicação atual utiliza arquivos fixos, a classe utiliza controle de concorrência para evitar que duas requisições usem os mesmos arquivos ao mesmo tempo.

---

### 4. LegacyCobol

Contém o programa COBOL `ALFA.cbl`.

O programa COBOL é responsável por:

- Ler o arquivo de entrada `request.txt`;
- Identificar a operação solicitada;
- Consultar dados de cliente;
- Atualizar telefone e e-mail;
- Persistir as alterações em `clientes.dat`;
- Gravar a resposta em `response.txt`.

Operações implementadas:

```text
CONSULTAR
ATUALIZAR
```

---

## Estrutura de comunicação com o COBOL

A comunicação entre o `zosproxy` e o COBOL usa um arquivo de entrada com campos de tamanho fixo.

### Formato do `request.txt`

```text
Campo      Tamanho
---------  -------
Operação   X(10)
ID         X(06)
Telefone   X(11)
E-mail     X(80)
```

Exemplo de consulta:

```text
CONSULTAR 000001
```

Exemplo de atualização:

```text
ATUALIZAR 00000162999990000cliente@email.com
```

O espaçamento é importante porque o COBOL lê os campos com tamanhos fixos.

---

### Formato do `response.txt`

O COBOL grava a resposta em formato separado por `|`.

```text
CodigoRetorno|Mensagem|ID|Nome|Telefone|Email
```

Exemplo:

```text
0000|Cliente encontrado.|000001|Maria Silva|62999990000|maria.silva@email.com
```

Códigos de retorno utilizados:

```text
0000 = Operação realizada com sucesso
0404 = Cliente não encontrado
0422 = Dados inválidos
0500 = Erro de sistema/processamento
```

---

## Endpoints da API

### Consultar cliente

```http
GET /api/clientes/{id}
```

Exemplo:

```bash
curl http://localhost:5005/api/clientes/000001
```

Resposta esperada:

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

### Atualizar contato

```http
PUT /api/clientes/{id}/contato
```

Exemplo:

```bash
curl -X PUT http://localhost:5005/api/clientes/000001/contato \
  -H "Content-Type: application/json" \
  -d '{"email":"novo.email@email.com","number":"62911112222"}'
```

Body esperado:

```json
{
  "email": "novo.email@email.com",
  "number": "62911112222"
}
```

---

## Armazenamento atual

Os dados dos clientes são armazenados no arquivo:

```text
LegacyCobol/data/clientes.dat
```

Formato do arquivo:

```text
ID|Nome|Telefone|Email
```

Exemplo:

```text
000001|Maria Silva|62999990000|maria.silva@email.com
000002|Joao Pereira|62333334444|joao.pereira@email.com
000003|Ana Costa|62888887777|ana.costa@email.com
```

Esse arquivo representa a persistência utilizada pelo componente legado nesta versão local do projeto.

---

## Como executar o projeto

### Pré-requisitos

- .NET instalado;
- GnuCOBOL instalado;
- Terminal Linux, WSL ou ambiente compatível;
- Porta `5005` disponível para o `zosproxy`.

---

### 1. Compilar o programa COBOL

Na raiz do projeto, execute:

```bash
cd LegacyCobol
cobc -x -free -o alfa ALFA.cbl
cd ..
```

Esse comando gera o executável `LegacyCobol/alfa`.

---

### 2. Executar o zosproxy

Em um terminal separado, execute:

```bash
dotnet run --project zosproxy/zosproxy.csproj
```

O `zosproxy` deve ficar disponível em:

```text
http://localhost:5005
```

Teste rápido:

```bash
curl http://localhost:5005/api/clientes/000001
```

---

### 3. Executar a interface web

Em outro terminal, execute:

```bash
dotnet run --project WebInterface/WebInterface.csproj
```

Depois, abra no navegador o endereço exibido pelo ASP.NET.

Na interface, use um ID existente, por exemplo:

```text
000001
```

---

## Como executar os testes

Com o `zosproxy` em execução, rode:

```bash
dotnet test tests/zosproxy.IntegrationTests/zosproxy.IntegrationTests.csproj
```

Os testes automatizados validam:

- Consulta de cliente existente;
- Consulta de cliente inexistente;
- Atualização de telefone e e-mail;
- Persistência dos dados após a atualização.

Fluxo testado:

```text
xUnit
  -> HTTP
     -> zosproxy
        -> COBOL
           -> clientes.dat
```

Esses testes são importantes porque validam a integração completa entre a API moderna e o componente legado.

---

## Decisões técnicas

### Uso de um proxy local

Foi criado o projeto `zosproxy` para simular localmente o papel de uma camada de integração no estilo z/OS Connect.

A interface web não chama o COBOL diretamente. Ela chama uma API REST, e a API é responsável por converter a requisição moderna para o formato esperado pelo legado.

---

### Uso de arquivos para comunicação com o COBOL

O `zosproxy` e o COBOL se comunicam através dos arquivos:

```text
LegacyCobol/io/request.txt
LegacyCobol/io/response.txt
```

Essa abordagem foi escolhida por ser simples, demonstrável em ambiente local e compatível com a ideia de integração com um componente legado.

---

### Uso de campos de tamanho fixo

O arquivo `request.txt` usa campos de tamanho fixo porque esse modelo é comum em sistemas COBOL e facilita o mapeamento entre estruturas modernas e estruturas legadas.

---

### Separação de responsabilidades

A solução foi organizada em camadas:

```text
WebInterface = interface com o usuário
zosproxy = API e integração
LocalCobolGateway = execução do COBOL
ALFA.cbl = regra de negócio legada
clientes.dat = persistência atual dos dados
```

Essa separação facilita a manutenção e deixa claro qual parte do sistema é responsável por cada etapa do fluxo.

---

## Relação com os requisitos do projeto

A solução atende aos principais requisitos funcionais:

- Consultar um cliente pelo ID;
- Exibir ID, nome, telefone e e-mail;
- Atualizar telefone e e-mail;
- Informar quando o cliente não existe;
- Persistir as alterações realizadas.

Também atende aos requisitos não funcionais principais:

- Utiliza .NET;
- Utiliza COBOL;
- Possui interface web;
- Possui estrutura organizada em camadas;
- Possui definição clara dos dados compartilhados entre os componentes;
- Possui testes automatizados de integração.

---

