# Работа с пользователями.

Данные пользователей хранятся в СУБД *PostgreSQL*. Пароли хешируются.


[Лимиты:](../src/versioning-manager-api/StaticStorages/FieldsLimits.cs)

* Максимальная длина имени пользователя - 20
* Максимальная длина пароля - 32
* Минимальная длина пароля - 4

## Роли:

Есть несколько [ролей](../src/versioning-manager-api/StaticStorages/RolesStorage.cs), на основе который регулируется доступ к АПИ методам.

Администратор системы может создать роль, включающую системные роли.

При первом запуске создается роль, указанная в конфигурационном файле, в которую добавляются все существующие системные роли.
