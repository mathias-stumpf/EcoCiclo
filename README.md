# EcoCiclo

Protótipo acadêmico em ASP.NET Core MVC para pesquisar resíduos e consultar orientações gerais de preparação e descarte. O conteúdo destaca que a coleta seletiva, seus dias e os materiais aceitos dependem das regras de cada município.

## Tecnologias

- C# e ASP.NET Core MVC (`net10.0`)
- Entity Framework Core 10
- SQLite
- Razor Views e CSS responsivo

## Executar

Requisitos: .NET SDK 10.

Na pasta do projeto:

```powershell
dotnet restore
dotnet run
```

Abra o endereço HTTP mostrado no terminal. Na inicialização, o aplicativo aplica as migrations pendentes e insere as categorias, resíduos e sinônimos de exemplo que ainda não existirem. O processo de seed é idempotente.

## Banco de dados

A conexão SQLite está em `appsettings.json` e o arquivo padrão é `database/ecociclo.db`.

Para consultar as migrations e aplicá-las manualmente:

```powershell
dotnet ef migrations list
dotnet ef database update
```

Para ver os resíduos cadastrados, abra `database/ecociclo.db` em um visualizador SQLite (por exemplo, uma extensão SQLite no VS Code) e execute:

```sql
SELECT Nome, Slug, Ativo
FROM residuos
ORDER BY Nome;
```

O arquivo `scripts/consultar-residuos.sql` contém essa consulta.

## Pesquisa

A página inicial pesquisa nomes e sinônimos persistidos no SQLite. A busca ignora diferenças entre maiúsculas e minúsculas, acentos e espaços repetidos. Os detalhes de cada resíduo ficam em `/residuos/{slug}`.

Os dados iniciais são exemplos educativos baseados nas fontes listadas em cada detalhe. As orientações são gerais; não indicam calendário, serviço ou ponto de coleta específico. Consulte as fontes oficiais locais para confirmar as regras aplicáveis.

## Escopo

Este protótipo não implementa mapas, geolocalização, APIs externas, pontos de coleta, autenticação ou painel administrativo. O banco mantém algumas tabelas da estrutura original, mas elas não são utilizadas pela interface deste protótipo.

## Migrations

- `InitialCreate`: estrutura inicial do banco.
- `AddResiduoNomeNormalizado`: campo e índice usados na pesquisa sem acentos.

Para criar uma migration após uma mudança de modelo:

```powershell
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```
