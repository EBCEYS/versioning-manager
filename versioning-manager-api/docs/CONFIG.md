# Описание конфигурационного файла

```json
{
  "Logging": { // настройки логирования
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "postgres": "User ID=admin;Password=admin;Host=localhost;Port=8890;Database=dev_db;Max Auto Prepare=100" // строка подключения к СУБД PostgreSQL
  },
  "JWTOptions": { // настройки JWT
    "Issuer": "https://my-server.ru",
    "Audience": "https://my-server.ru",
    "SecretFilePath": "/secrets/jwt.key", // путь до файла с ключем для JWT
    "TokenTimeToLive": "00:30:00" // время жизни токена. Опционально. Если не указано, то валидация лайфтайма будет отключена.
  },
  "ApiKeyOptions": { // настройки апи ключей
    "CryptKeyFilePath": "/secrets/api.key", // файл с ключем для шифрования апи ключа
    "CryptIVFilePath": "/secrets/iv.key",
    "Prefix": "ebvm-" // префикс апи ключа
  },
  "DefaultUser": { // настройки пользователя, создаваемого при первом запуске системы. Пользователь создается со всем ролями.
    "DefaultUsername": "admin",
    "DefaultPassword": "admin",
    "DefaultRoleName": "root"
  },
  "DockerClient": { // настройки подключения к докеровскому сокету
    "UseDefaultConnection": true,
    "DockerHost": "unix://var/run/docker.sock", // путь до сокета докера если. Читается если UseDefaultConnection = false
    "ConnectionTimeout": "00:00:10",
    "Credentials": { // ОПЦИОНАЛЬНО. Данные для подключения к докеровскому сокету.
        "Username":"",
        "PasswordFile":"", // путь до файла с паролем
        "UseTls":false
    }
  },
  "GitlabRegistry": { // настройки подключения к вашему gitlab
    "Address": "registry.gitlab-dev.loc",
    "Username": "root",
    "KeyFile": "/secrets/git.key" // файл с gitlab ключем
  }
}
```