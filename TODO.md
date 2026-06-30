- [x] Inspect existing Waiver/Person FK configuration and identify which migration created `FK_Waivers_Persons_PersonId`
- [x] Add EF model configuration to set Waiver->Person delete behavior to `DeleteBehavior.NoAction`
- [x] Create new migration to drop `FK_Waivers_Persons_PersonId` (if exists) and recreate it with `ON DELETE NO ACTION`

- [x] Ensure new migration also handles duplicate FK name variants (e.g. `...PersonId1`)
- [x] Update/verify `ApplicationDbContextModelSnapshot` is consistent (run `dotnet ef migrations add` if needed)
- [x] Run `dotnet ef database update` and confirm the FK creation error is resolved
