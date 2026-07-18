# Password Authentication V2 - Development override

`Password:MinLength` defaults to `15` in `appsettings.json` and must remain at least `15` outside `Development`.

For local manual testing only, set this in the untracked `Presentation/Ecom.API/appsettings.Local.json`:

```json
{
  "Password": { "MinLength": 5 }
}
```

The application rejects a value below `15` when the environment is not `Development`. Restore or remove the local override before testing production-like behavior. This option does not permit plaintext password storage, bypass BCrypt, or weaken rate limits.
