
http://localhost:5100 -  IdentityServer (project - api)
http://localhost:5200 - HRSafety.API (project - api)
http://localhost:5300 - HRSafety.WEB (project - web)

Migration:
PM>  add-migration -n InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Migrations/IdentityServer/PersistedGrantDb -StartupProject IdentityServer
PM>  add-migration -n InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Migrations/IdentityServer/ConfigurationDb -StartupProject IdentityServer
PM>  add-migration -n InitialAspNetIdentity -c IdentityServerDbContext -o Migrations -StartupProject IdentityServer
PM> update-database -StartupProject IdentityServer


IdentityServer4
AspNetIdentity
Migrations (IdentityServer4 and AspNetIdentity)









