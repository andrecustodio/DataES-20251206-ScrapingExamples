# Pré-requisitos - Workshop Playwright.NET

Este documento lista todos os requisitos necessários para executar os exemplos deste workshop.

## Requisitos do Sistema

### .NET SDK

- **Versão mínima**: .NET 6.0
- **Versão recomendada**: .NET 10.0
- **Download**: https://dotnet.microsoft.com/download

Para verificar a instalação:
```bash
dotnet --version
```

### Playwright.NET

O Playwright.NET requer a instalação dos navegadores. Execute os seguintes comandos:

```bash
# Instalar a ferramenta CLI do Playwright
dotnet tool install --global Microsoft.Playwright.CLI

# Instalar os navegadores (Chromium, Firefox, WebKit)
playwright install
```

**Nota**: A instalação dos navegadores pode demorar alguns minutos e requer aproximadamente 1GB de espaço em disco.

### Docker e Docker Compose

- **Docker Desktop** (Windows/Mac) ou **Docker Engine** (Linux)
- **Versão mínima**: Docker 20.10+
- **Download**: https://www.docker.com/get-started

Para verificar a instalação:
```bash
docker --version
docker-compose --version
```

**Nota**: Os serviços de infraestrutura (Seq, Prometheus, Grafana) e as páginas alvo são executados via Docker Compose.

### Node.js (Opcional)

Necessário apenas se você quiser modificar ou executar localmente as páginas alvo Vue.js:

- **Versão recomendada**: Node.js 18.x ou superior
- **Download**: https://nodejs.org/

Para verificar a instalação:
```bash
node --version
npm --version
```

## IDEs e Editores Recomendados

Qualquer um dos seguintes IDEs funcionará bem:

- **Visual Studio 2022** (Community, Professional ou Enterprise)
- **Visual Studio Code** com extensão C#
- **JetBrains Rider**

## Estrutura de Portas

Os seguintes serviços serão iniciados via Docker Compose (portas serão atribuídas automaticamente):

- **Static Grid Server**: Porta HTTP para página estática
- **Vue Grid App**: Porta HTTP para aplicação Vue.js
- **Seq**: Porta HTTP para interface web de logs
- **Prometheus**: Porta HTTP para métricas
- **Prometheus Pushgateway**: Porta HTTP para receber métricas
- **Grafana**: Porta HTTP para dashboards

**Nota**: As portas específicas serão exibidas ao iniciar o docker-compose. Verifique a saída do comando `docker-compose up` para ver as portas atribuídas.

## Espaço em Disco

Recomenda-se pelo menos **5GB** de espaço livre para:
- Navegadores do Playwright (~1GB)
- Imagens Docker (~2GB)
- Código fonte e dependências (~500MB)
- Dados gerados (~500MB)

## Sistema Operacional

Os exemplos foram testados e funcionam em:
- Windows 10/11
- macOS 10.15+
- Linux (Ubuntu 20.04+, Debian 11+, etc.)

## Verificação Rápida

Execute os seguintes comandos para verificar se tudo está configurado corretamente:

```bash
# Verificar .NET
dotnet --version

# Verificar Playwright
playwright --version

# Verificar Docker
docker --version
docker-compose --version

# Verificar se os navegadores estão instalados
playwright install --dry-run
```

## Próximos Passos

Após verificar todos os pré-requisitos:

1. Clone ou baixe este repositório
2. Navegue até a pasta do projeto
3. Inicie a infraestrutura:
   ```bash
   cd source-pages
   docker-compose up -d
   ```
4. Aguarde alguns segundos para todos os serviços iniciarem
5. Comece com o exercício 01-BasicSetup

## Solução de Problemas

### Erro ao instalar navegadores do Playwright

Se encontrar problemas ao instalar os navegadores, tente:
```bash
playwright install --with-deps
```

### Docker não inicia

- Verifique se o Docker Desktop está em execução (Windows/Mac)
- Verifique se o serviço Docker está rodando (Linux)
- Tente executar `docker ps` para verificar se o Docker está funcionando

### Portas já em uso

Se alguma porta já estiver em uso, o Docker Compose atribuirá automaticamente uma porta alternativa. Verifique a saída do `docker-compose up` para ver as portas reais.

## Suporte

Para problemas específicos de cada exercício, consulte o README.md dentro da pasta do exercício correspondente.


