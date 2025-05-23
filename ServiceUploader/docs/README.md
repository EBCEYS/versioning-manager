# ServiceUploader

Сервис для работы с [сервером версионирования](../../versioning-manager-api/docs).

## Функционал

Сервис позволяет работать с сервером версионирования, а именно:

1. загружает информацию о образах сервисов на сервер;
1. выгружает сборки приложений с сервера;
1. лоадит образы в *Docker*.

## Ключи:

Есть три основных режима работы, который переключаются обязательным ключем *-m/--mode*:

* *Load* - загрузка информации на сервер.
* *Save* - выгрузка образов и *docker-compose* файла проекта.
* *Update* - лоадинг образов из текущей директории в локальный *docker*.

Для вывода подсказки: *-?/-h/--help*.

Версия: *--version*.

## *Load*

На сервер загружается информация по сервису в рамках проекта.

### Ключи:

* *-t/--token* - токен доступа устройства к серверу версионирования. Переменная окружения - *SERVICEUPLOADER_TOKEN*.
* *--uri* - адрес сервера версионирования. Переменная окружения - *SERVICEUPLOADER_VM_URI*.
* *--image* - имя тега образа сервиса, по которому нужно загрузить инф-цию. Переменная окружения - *SERVICEUPLOADER_IMAGE_TAG*.
* *--project-name* - имя проекта, в который нужно добавить новый образ. Переменная окружения - *SERVICEUPLOADER_PROJECT_NAME*.
* *--service-name* - имя сервиса, который нужно загрузить. Переменная окружения - *SERVICEUPLOADER_SERVICE_NAME*.
* *--service-version* - версия сервиса. Переменная окружения - *SERVICEUPLOADER_IMAGE_VERSION*.
* *--compose-file* - опциональный ключ. Путь до *docker-compose* файла с инструкцией по запуску сервиса. Переменная окружения - *SERVICEUPLOADER_DOCKER_COMPOSE_FILE*.

## *Save*

С сервера выгружаются все образы, связанные с выбранным проектом и *docker-compose* файл.

### Ключи:

* *-t/--token* - токен доступа устройства к серверу версионирования. Переменная окружения - *SERVICEUPLOADER_TOKEN*.
* *--uri* - адрес сервера версионирования. Переменная окружения - *SERVICEUPLOADER_VM_URI*.
* *--project-name* - имя проекта, который нужно выгрузить. Переменная окружения - *SERVICEUPLOADER_PROJECT_NAME*.

## *Update*

Лоадит все **.tar* образы из текущей дириктории в *docker*.

### Ключи:

* *--uri* - опциональный ключ. Путь до докеровского сокета. Переменная окружения - *SERVICEUPLOADER_VM_URI*.