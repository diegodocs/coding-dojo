# Order Refactor

## Background

This is a fun kata that you will be responsible to refactor a real legacy Order System. It is a great opportunity to apply OOP, Design Patterns SOLID patterns with Test Coverage (TDD).

## Instructions

1. Complete your local env [**steps**](#setup-your-local-environment)
1. Complete each iteration before reading the next one.

## Iteration One

Based on follow existing code:

1. create unit-test to cover scenarios:

- Pedido realizado com sucesso
- Cliente invalido
- Pedido invalido  
- PedidoItem invalido  

## Iteration Two

1. improve error with specific messages
1. move entities to Domain layer
1. a rich Domain should be responsible for validation rules (show state and list of errors)
1. SalvarPedido should raise exception if entities are not valid before touch the database

## Iteration Three

1. you should always close connection, even when error happen
1. implement Repository Pattern
1. implement IoC/Dependency Injection
1. implement in-memory repository

## Iteration Four

1. Infra layer should isolate every external dependency
1. if SendEmail fail, you can retry later ... but the order can save normally
1. create mocks for infra classes
1. improve table design isolating products info

### Setup your local environment

#### Fake SMTP

```powershell
docker run --rm -it -p 5000:80 -p 2525:25 rnwood/smtp4dev
```

#### SQLServer

```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=abcDEF123#" -p 1433:1433 --name sqlsv -d mcr.microsoft.com/mssql/server:2022-latest
```

#### Create Database

```sql
    CREATE DATABASE orders
```

#### Create Tables

```sql
 
    CREATE TABLE [Pedido] (
        [PedidoId]            INT             NOT NULL,
        [ClienteID]           INT             NOT NULL,
        [PedidoDataCadastro]  DATETIME        NULL,
        [PedidoValorTotal]    NUMERIC (18, 2) NULL,  
        PRIMARY KEY ([PedidoId] ASC)
    )

    CREATE TABLE [Cliente] (
        [ClienteID]           INT          NOT NULL,
        [ClienteNome]         VARCHAR (50) NULL,
        [ClienteEmail]        VARCHAR (50) NULL,
        [ClienteCPF]          VARCHAR (50) NULL,
        [ClienteDataCadastro] DATETIME     NULL,
        PRIMARY KEY  ([ClienteID] ASC)
    )

    CREATE TABLE [PedidoItem] (
        [PedidoItemId]  INT             NOT NULL,
        [ValorUnitario] NUMERIC (18, 2) NULL,
        [Quantidade]    INT             NULL,
        [NomeProduto]   VARCHAR (50)    NULL,
        [CodigoProduto] INT             NULL,
        PRIMARY KEY ([PedidoItemId] ASC)
        )
```

#### Example of valid order

```csharp
    
    var pedido = new Pedido();
    pedido.ClienteNome = "Cliente 1";   
    pedido.ClienteCPF = "12345678901";
    pedido.ClienteEmail = "cliente1@email.com";
    pedido.ClienteDataCadastro = DateTime.Now;
    pedido.PedidoDataCadastro = DateTime.Now;
    pedido.ListaDeItens.Add(new PedidoItem { CodigoProduto = 1, NomeProduto = "Camisa Futebol Brasil Tamanho G", Quantidade = 10, ValorUnitario = 299 });
    pedido.ListaDeItens.Add(new PedidoItem { CodigoProduto = 2, NomeProduto = "Raquete Ping Pong Profissional", Quantidade = 2, ValorUnitario = 450 });
    pedido.ListaDeItens.Add(new PedidoItem { CodigoProduto = 3, NomeProduto = "Tenis Mizuno Prophecy LS Tamanho 36", Quantidade = 1, ValorUnitario = 1999 });
    var resultado = pedido.SalvarPedido();
    
```

#### Code for refactoring

```csharp
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Mail;

namespace CodingDojo
{
    public class PedidoItem
    {
        public int Quantidade { get; set; }
        public string NomeProduto { get; set; }
        public int CodigoProduto { get; set; }
        public decimal ValorUnitario { get; set; }
    }

    public class Pedido
    {
        public int PedidoId { get; set; } = 0;
        public DateTime PedidoDataCadastro { get; set; }
        public decimal PedidoValorTotal { get; set; } = 0;
        public int ClienteId { get; set; } = 0;
        public string ClienteNome { get; set; } = "";
        public string ClienteEmail { get; set; } = "";
        public string ClienteCPF { get; set; } = "";
        public DateTime ClienteDataCadastro { get; set; }
        public List<PedidoItem> ListaDeItens { get; set; } = new List<PedidoItem>();
        public string SalvarPedido()
        {
            var conexao = new SqlConnection();
            conexao.ConnectionString = "Server=localhost;Database=orders;Trust Server Certificate=True;MultipleActiveResultSets=true;User ID=sa;Password=abcDEF123#";
            conexao.Open();

            if (!ClienteNome.Contains(" "))
            {
                return "Cliente inválido";
            }

            if (!ClienteEmail.Contains("@"))
            {
                return "Cliente inválido";
            }

            if (ClienteCPF.Length != 11)
            {
                return "Cliente inválido";
            }

            var comando1 = new SqlCommand();
            comando1.Connection = conexao;
            comando1.CommandType = CommandType.Text;

            comando1.CommandText = "SELECT CLIENTEID FROM CLIENTE WHERE CLIENTECPF = @CLIENTECPF";
            comando1.Parameters.AddWithValue("ClienteCPF", ClienteCPF);
            var resultado = comando1.ExecuteScalar();

            if (resultado is System.DBNull)
            {
                comando1.CommandText = "SELECT MAX(CLIENTEID)+1 FROM CLIENTE ";
                var resultado1 = comando1.ExecuteScalar();
                ClienteId = resultado1 is System.DBNull ? 1 : (int)resultado1;

                comando1.Parameters.Clear();
                comando1.CommandText = "INSERT INTO CLIENTE (ClienteId, ClienteNome, ClienteEmail, ClienteCPF, ClienteDataCadastro) VALUES (@ClienteId, @ClienteNome, @ClienteEmail, @ClienteCPF, @ClienteDataCadastro)";
                comando1.Parameters.AddWithValue("ClienteId", ClienteId);
                comando1.Parameters.AddWithValue("ClienteNome", ClienteNome);
                comando1.Parameters.AddWithValue("ClienteEmail", ClienteEmail);
                comando1.Parameters.AddWithValue("ClienteCPF", ClienteCPF);
                comando1.Parameters.AddWithValue("ClienteDataCadastro", ClienteDataCadastro);
                comando1.ExecuteNonQuery();
            }
            else
            {
                ClienteId =  (int)resultado;
            }

            comando1.CommandText = "SELECT MAX(PEDIDOID)+1 FROM PEDIDO ";
            var resultado2 = comando1.ExecuteScalar();
            PedidoId = resultado2 is System.DBNull ? 1 : (int)resultado2;
            PedidoValorTotal = ListaDeItens.Sum(x => x.Quantidade * x.ValorUnitario);

            if (PedidoValorTotal <= 0)
            {
                return "Pedido inválido";
            }

            comando1.Parameters.Clear();
            comando1.CommandText = "INSERT INTO PEDIDO (PedidoId, ClienteId, PedidoDataCadastro, PedidoValorTotal) VALUES (@PedidoId, @ClienteId, @PedidoDataCadastro, @PedidoValorTotal)";
            comando1.Parameters.AddWithValue("PedidoId", PedidoId);
            comando1.Parameters.AddWithValue("ClienteId", ClienteId);
            comando1.Parameters.AddWithValue("PedidoDataCadastro", PedidoDataCadastro);
            comando1.Parameters.AddWithValue("PedidoValorTotal", PedidoValorTotal);
            comando1.ExecuteNonQuery();            
            comando1.Parameters.Clear();
            comando1.CommandText = "SELECT MAX(PEDIDOITEMID)+1 FROM PEDIDOITEM ";
            var resultado3 = comando1.ExecuteScalar();
            var pedidoItemId = resultado3 is System.DBNull ? 1 : (int)resultado3;

            foreach (var item in ListaDeItens)
            {

                if (item.Quantidade == 0)
                {
                    return "PedidoItem inválido";
                }

                if (item.ValorUnitario == 0)
                {
                    return "PedidoItem inválido";
                }

                if (item.CodigoProduto == 0)
                {
                    return "PedidoItem inválido";
                }

                if (item.NomeProduto == "")
                {
                    return "PedidoItem inválido";
                }

                comando1.Parameters.Clear();
                comando1.CommandText = "INSERT INTO PEDIDOITEM (PEDIDOITEMID,CODIGOPRODUTO, NOMEPRODUTO, QUANTIDADE, VALORUNITARIO) VALUES (@PEDIDOITEMID,@CODIGOPRODUTO, @NOMEPRODUTO, @QUANTIDADE, @VALORUNITARIO)";
                comando1.Parameters.AddWithValue("PEDIDOITEMID", pedidoItemId);
                comando1.Parameters.AddWithValue("CODIGOPRODUTO", item.CodigoProduto);
                comando1.Parameters.AddWithValue("NOMEPRODUTO", item.NomeProduto);
                comando1.Parameters.AddWithValue("QUANTIDADE", item.Quantidade);
                comando1.Parameters.AddWithValue("VALORUNITARIO", item.ValorUnitario);
                comando1.ExecuteNonQuery();

                pedidoItemId++;
            }

            var mail = new MailMessage("codingdojo@sistema.com", ClienteEmail);
            var client = new SmtpClient
            {
                Port = 2525,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "localhost",

            };

            mail.Subject = $"Pedido {PedidoId} realizado com sucesso";
            mail.Body = $"Parabéns {ClienteNome}! Seu pedido {PedidoId} no valor de R${PedidoValorTotal},00 vai chegar bem antes do que você espera ... ou não.";
            client.Send(mail);

            return "Pedido realizado com sucesso!";
        }
    }
}
```
