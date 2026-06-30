# Sub-Users (Agent Accounts) — Full Implementation Document
### Digital Loan Platform 2  |  June 2026

---

## What Are We Building?

Right now, every person who registers on the platform is a **standalone user** — they see only their own data.

We are adding the ability for a **Parent User (Manager)** to create **Sub-Users (Agents)** that work under them.

### How It Works

```
Manager (Parent)
  ├── Agent 1  → logs in, creates loans, sees only their own data
  ├── Agent 2  → logs in, creates loans, sees only their own data
  └── Agent 3  → logs in, creates loans, sees only their own data

Manager → can see ALL data across all their agents (aggregated view)
Manager → can add / deactivate / delete agents at any time
```

### What Changes for Agents?
- Agents log in with their own email and password
- Agents can do all loan operations (borrowers, applications, payments, etc.)
- Agents **cannot** manage other users or create sub-users
- Agents are always **owned by** the Manager who created them

---

## The Plan at a Glance

| Phase | What | Files Touched |
|-------|------|---------------|
| **1** | Database — add parent link to User table | `User.cs` + EF Migration |
| **2** | Identity Claims — stamp parent info at login | `CustomUserClaimsPrincipalFactory.cs`, `IUserContext.cs`, `UserContext.cs` |
| **3** | DTO — new data shape for creating an agent | `IdentityDTO.cs` |
| **4** | Service Contract — define what operations exist | `IIdentity.cs` |
| **5** | Service Implementation — write the actual logic | `IdentityRepository.cs` |
| **6** | UI — My Team page + dialog + nav link | 2 new Razor pages + `MainLayout.razor` |

---

---

## Phase 1 — Database Change

**Goal:** Add a `ParentUserId` column to the `AspNetUsers` table so a user can be linked to their parent.

### File: `Infrastructure/Identity/User.cs`

**Before:**
```csharp
public class User : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
```

**After (add 3 lines):**
```csharp
public class User : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; }

    // Sub-user support — null means this is a top-level Manager account
    public int? ParentUserId { get; set; }
    public User ParentUser { get; set; }
    public ICollection<User> SubUsers { get; set; } = new List<User>();

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
```

> **Why nullable?** A Manager has no parent, so `ParentUserId = null`. An Agent always has `ParentUserId = <manager's Id>`.

### Run Migration

After saving the file, run these two commands in the terminal:

```bash
dotnet ef migrations add AddSubUserSupport --project Infrastructure --startup-project Web
dotnet ef database update --project Infrastructure --startup-project Web
```

This adds a `ParentUserId` column to the database without touching any existing data.

---

---

## Phase 2 — Identity Claims

**Goal:** When a user logs in, store their `ParentUserId` inside their login token (claims) so any page can read it instantly — without hitting the database.

### File: `Infrastructure/Identity/CustomUserClaimsPrincipalFactory.cs`

**Before:**
```csharp
protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
{
    var identity = await base.GenerateClaimsAsync(user);
    identity.AddClaim(new Claim("FirstName", user.FirstName ?? string.Empty));
    identity.AddClaim(new Claim("LastName", user.LastName ?? string.Empty));
    return identity;
}
```

**After (add 4 lines inside the method):**
```csharp
protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
{
    var identity = await base.GenerateClaimsAsync(user);
    identity.AddClaim(new Claim("FirstName", user.FirstName ?? string.Empty));
    identity.AddClaim(new Claim("LastName", user.LastName ?? string.Empty));

    // Stamp parent info so sub-user pages know who owns this account
    if (user.ParentUserId.HasValue)
        identity.AddClaim(new Claim("ParentUserId", user.ParentUserId.Value.ToString()));

    return identity;
}
```

---

### File: `Application/Interfaces/IUserContext.cs`

**Add 2 new properties** to the interface:

```csharp
public interface IUserContext
{
    int? Id { get; }
    bool IsAuthenticated { get; }
    string Email { get; }
    string FirstName { get; }
    string LastName { get; }
    string FullName { get; }
    string Initials { get; }

    // NEW — Sub-user support
    int? ParentUserId { get; }   // null = this user IS a parent (Manager)
    bool IsSubUser { get; }      // true = this user is an Agent under a Manager
}
```

---

### File: `Infrastructure/Identity/UserContext.cs`

**Add 2 new properties** at the bottom of the class (before the closing brace):

```csharp
/// <inheritdoc />
public int? ParentUserId =>
    int.TryParse(ClaimsPrincipal?.FindFirst("ParentUserId")?.Value, out var id) ? id : null;

/// <inheritdoc />
public bool IsSubUser => ParentUserId.HasValue;
```

---

---

## Phase 3 — New DTO

**Goal:** Define the shape of data needed to create a new sub-user (agent).

### File: `Application/DTO/IdentityDTO.cs`

**Add this class at the bottom** (before the closing `}`):

```csharp
/// <summary>
/// Data required to create a new sub-user (Agent) under a parent Manager.
/// </summary>
public class CreateSubUserDTO
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = string.Empty;
}
```

Also update **`UserDetailDTO`** to include parent info (so the My Team page can display it):

```csharp
public class UserDetailDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // NEW
    public int? ParentUserId { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials
    {
        get
        {
            var first = !string.IsNullOrEmpty(FirstName) ? FirstName[0].ToString().ToUpper() : "";
            var last  = !string.IsNullOrEmpty(LastName)  ? LastName[0].ToString().ToUpper()  : "";
            return $"{first}{last}";
        }
    }
}
```

---

---

## Phase 4 — Service Contract

**Goal:** Declare the new sub-user operations in the interface so any part of the app can use them.

### File: `Application/Interfaces/IIdentity.cs`

**Add 4 new method signatures:**

```csharp
public interface IIdentity
{
    // Existing methods (do not change)
    Task RegisterUser(RegisterUserDTO dto);
    Task<List<UserDetailDTO>> GetAllUsers();
    Task<UserDetailDTO> GetUserById(int id);
    Task UpdateUser(int id, UserDetailDTO dto);
    Task<bool> LoginAsync(LoginDTO dto);
    Task LogoutAsync();

    // NEW — Sub-user (Agent) management
    /// <summary>Creates a new agent account owned by the given parent Manager.</summary>
    Task CreateSubUserAsync(CreateSubUserDTO dto, int parentUserId);

    /// <summary>Returns all agents belonging to a specific Manager.</summary>
    Task<List<UserDetailDTO>> GetSubUsersAsync(int parentUserId);

    /// <summary>Activates or deactivates an agent account.</summary>
    Task SetSubUserStatusAsync(int subUserId, bool isActive, int parentUserId);

    /// <summary>Permanently deletes an agent (only if owned by this Manager).</summary>
    Task DeleteSubUserAsync(int subUserId, int parentUserId);
}
```

---

---

## Phase 5 — Service Implementation

**Goal:** Write the actual code that creates, reads, and manages sub-users in the database.

### File: `Infrastructure/Identity/IdentityRepository.cs`

**Add these 4 methods** inside the `IdentityRepository` class (before the last `}`):

```csharp
public async Task CreateSubUserAsync(CreateSubUserDTO dto, int parentUserId)
{
    // Check email is not taken
    var existing = await _userManager.FindByEmailAsync(dto.Email);
    if (existing != null)
        throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

    // Create a Person record (same pattern as RegisterUser)
    var person = new Person
    {
        FirstName  = dto.FirstName,
        LastName   = dto.LastName,
        Email      = dto.Email,
        phoneNumber = dto.PhoneNumber,
        Status     = "Active",
        Sex        = "Unknown",
        DateOfBirth = DateTime.Now,
        Country    = string.Empty,
        CreatedBy  = dto.Email,
        UpdateBy   = dto.Email,
    };
    _dbContext.Persons.Add(person);
    await _dbContext.SaveChangesAsync();

    // Create the Identity user, linked to the parent
    var user = new User
    {
        FirstName     = dto.FirstName,
        LastName      = dto.LastName,
        Email         = dto.Email,
        UserName      = dto.Email,
        PhoneNumber   = dto.PhoneNumber,
        PersonId      = person.Id,
        ParentUserId  = parentUserId,   // ← Key link
        EmailConfirmed = true,
        CreatedAt     = DateTime.UtcNow,
        UpdatedAt     = DateTime.UtcNow,
    };

    var result = await _userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded)
    {
        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"Failed to create agent: {errors}");
    }

    // Give the sub-user the Agent role
    await _userManager.AddToRoleAsync(user, "Agent");
}

public async Task<List<UserDetailDTO>> GetSubUsersAsync(int parentUserId)
{
    var subUsers = await _userManager.Users
        .Where(u => u.ParentUserId == parentUserId)
        .OrderBy(u => u.LastName)
        .ToListAsync();

    return subUsers.Select(u => new UserDetailDTO
    {
        Id           = u.Id,
        FirstName    = u.FirstName ?? "",
        LastName     = u.LastName  ?? "",
        Email        = u.Email     ?? "",
        PhoneNumber  = u.PhoneNumber,
        EmailConfirmed = u.EmailConfirmed,
        ParentUserId = u.ParentUserId,
        IsActive     = u.EmailConfirmed,   // use EmailConfirmed as active flag
        CreatedAt    = u.CreatedAt,
        UpdatedAt    = u.UpdatedAt,
    }).ToList();
}

public async Task SetSubUserStatusAsync(int subUserId, bool isActive, int parentUserId)
{
    var user = await _userManager.FindByIdAsync(subUserId.ToString());
    if (user == null || user.ParentUserId != parentUserId)
        throw new InvalidOperationException("Agent not found or access denied.");

    user.EmailConfirmed = isActive;   // use EmailConfirmed as active/inactive flag
    user.UpdatedAt = DateTime.UtcNow;
    await _userManager.UpdateAsync(user);
}

public async Task DeleteSubUserAsync(int subUserId, int parentUserId)
{
    var user = await _userManager.FindByIdAsync(subUserId.ToString());
    if (user == null || user.ParentUserId != parentUserId)
        throw new InvalidOperationException("Agent not found or access denied.");

    var result = await _userManager.DeleteAsync(user);
    if (!result.Succeeded)
    {
        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"Failed to delete agent: {errors}");
    }
}
```

---

---

## Phase 6 — User Interface

### 6.1 — New Page: `Web/Components/Pages/MyTeam.razor`

This page lists all agents under the logged-in Manager, with buttons to add, deactivate, and delete them.

**What it looks like:**
```
┌──────────────────────────────────────────────────────────┐
│  My Team (Agents)                          + Add Agent   │
├──────────────────────────────────────────────────────────┤
│  Avatar  Name          Email            Status  Actions  │
│  JD      John Doe      john@mail.com    ● Active  [⛔][🗑]│
│  JS      Jane Smith    jane@mail.com    ● Active  [⛔][🗑]│
└──────────────────────────────────────────────────────────┘
```

### 6.2 — New Dialog: `Web/Components/Pages/CreateAgentDialog.razor`

A popup form with these fields:
- First Name
- Last Name
- Email
- Phone Number
- Password

On submit → calls `CreateSubUserAsync` → closes dialog → refreshes the list.

### 6.3 — Nav Link in `MainLayout.razor`

Add a "My Team" link to the sidebar — **only visible to Managers**:

```razor
<AuthorizeView Roles="Manager">
    <Authorized>
        <MudNavLink href="/MyTeam" Icon="@Icons.Material.Outlined.Group">
            @(_drawerOpen ? "My Team" : "")
        </MudNavLink>
    </Authorized>
</AuthorizeView>
```

---

---

## Summary Checklist

```
Phase 1 — Database
  [ ] Add ParentUserId, SubUsers to User.cs
  [ ] Run EF migration: AddSubUserSupport

Phase 2 — Claims
  [ ] Stamp ParentUserId claim in CustomUserClaimsPrincipalFactory.cs
  [ ] Add ParentUserId, IsSubUser to IUserContext.cs
  [ ] Implement ParentUserId, IsSubUser in UserContext.cs

Phase 3 — DTO
  [ ] Add CreateSubUserDTO to IdentityDTO.cs
  [ ] Add ParentUserId, IsActive to UserDetailDTO

Phase 4 — Interface
  [ ] Add 4 method signatures to IIdentity.cs

Phase 5 — Implementation
  [ ] Add 4 methods to IdentityRepository.cs

Phase 6 — UI
  [ ] Create MyTeam.razor page
  [ ] Create CreateAgentDialog.razor dialog
  [ ] Add nav link to MainLayout.razor
```

---

> **Ready to code?** Say **"implement phase 1"** through **"implement phase 6"** one at a time,
> or say **"implement all"** and the entire feature will be written for you.
