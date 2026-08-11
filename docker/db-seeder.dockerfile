# Runs the database migration and seed SQL scripts.
FROM mcr.microsoft.com/powershell:7.5-debian-12

WORKDIR /scripts
ENV LANG=C.UTF-8
ENV LC_ALL=C.UTF-8

# psql is the only database tool needed in this container.
RUN apt-get update \
    && apt-get install -y --no-install-recommends postgresql-client \
    && rm -rf /var/lib/apt/lists/*

COPY ./scripts/initialize-database.ps1 ./initialize-database.ps1
COPY ./scripts/Seeder.sql ./Seeder.sql

ENTRYPOINT ["pwsh", "-NoLogo", "-NoProfile", "-File", "./initialize-database.ps1"]
CMD ["-PostgresHost", "db", "-PostgresPort", "5432"]
