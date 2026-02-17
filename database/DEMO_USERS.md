# Demo User Login Credentials

## Quick Reference for Off the Grid 2025

### Administrator
```
Email: admin@blackbaud.com
Name: Sarah Administrator
Role: Admin
```
**Access**: Full system access, manage hackathons, users, and all content

---

### Judges (3)

#### Judge 1
```
Email: judge1@blackbaud.com
Name: Michael Chen
Role: Judge
```

#### Judge 2
```
Email: judge2@blackbaud.com
Name: Jennifer Rodriguez
Role: Judge
```

#### Judge 3
```
Email: judge3@blackbaud.com
Name: David Patel
Role: Judge
```

**Access**: View all ideas, submit ratings and feedback, participate in discussions

---

### Participants (10)

#### Team Leaders

```
Email: alex.johnson@blackbaud.com
Name: Alex Johnson
Team: Aqua Innovators (Lead)
Idea: Solar-Powered Water Purification System 🏆
```

```
Email: emma.williams@blackbaud.com
Name: Emma Williams
Team: WindTech Collective (Lead)
Idea: Wind-Powered IoT Mesh Network 🏆
```

```
Email: marcus.thompson@blackbaud.com
Name: Marcus Thompson
Team: BioEnergy Squad (Lead)
Idea: Biogas Generator for Rural Kitchens
```

```
Email: sophia.garcia@blackbaud.com
Name: Sophia Garcia
Team: ColdChain Champions (Lead)
Idea: Off-Grid Cold Storage for Farmers
```

```
Email: liam.martinez@blackbaud.com
Name: Liam Martinez
Team: MobilePower Pioneers (Lead)
Idea: Portable Renewable Energy Backpack
```

```
Email: noah.thomas@blackbaud.com
Name: Noah Thomas
Team: GridSmart AI (Lead)
Idea: AI-Powered Energy Management 🏆
```

#### Solo Participants

```
Email: olivia.anderson@blackbaud.com
Name: Olivia Anderson
Idea: Micro-Hydro Power for Mountain Communities
```

```
Email: ava.jackson@blackbaud.com
Name: Ava Jackson
Idea: Composting Toilet with Biochar Production (Draft)
```

#### Team Members (No Ideas)

```
Email: ethan.white@blackbaud.com
Name: Ethan White
Team: Aqua Innovators (Member)
```

```
Email: isabella.harris@blackbaud.com
Name: Isabella Harris
Team: WindTech Collective (Member)
```

**Access**: Submit ideas, create teams, comment, view all ideas

---

## Authentication Notes

**Important**: This platform uses Blackbaud OAuth authentication. The seeded email addresses are for demo purposes. In a real environment:

1. Users authenticate via Blackbaud SSO
2. Email addresses match Blackbaud account emails
3. Roles are assigned after first login

For **local development/testing**:
- JWT tokens can be generated for these email addresses
- Or implement a test authentication bypass
- Or use the actual Blackbaud OAuth flow with test accounts

---

## Quick Login Guide

### Step 1: Choose a Persona

- **Administrator**: Test admin dashboard, user management, hackathon creation
- **Judge**: Test rating interface, judging workflows, idea evaluation
- **Participant**: Test idea submission, team management, commenting

### Step 2: Login

Visit the application and authenticate with Blackbaud OAuth using one of the emails above.

### Step 3: Explore

Based on your role:

**Admin Dashboard**:
- View analytics for "Off the Grid 2025"
- See 8 ideas, 18 ratings, 11 comments
- Manage users and hackathon settings

**Judge Interface**:
- View submitted ideas (7 total)
- See existing ratings from other judges
- Add additional ratings and feedback

**Participant View**:
- Browse idea gallery
- View team dashboards
- Submit comments
- Create new ideas

---

## Demo Scenarios

### Scenario 1: Judge Evaluation
**Login as**: judge1@blackbaud.com (Michael Chen)
**Action**: Review unrated ideas and submit feedback
**Result**: See how ratings aggregate and appear in analytics

### Scenario 2: Team Collaboration
**Login as**: alex.johnson@blackbaud.com (Alex - Team Lead)
**Action**: View team dashboard, see team member contributions
**Alternative**: Login as ethan.white@blackbaud.com (Team Member)
**Result**: Different perspectives on same team

### Scenario 3: Idea Submission
**Login as**: ava.jackson@blackbaud.com (Ava)
**Action**: Complete the draft idea "Composting Toilet" and submit
**Result**: See submission workflow and email notifications

### Scenario 4: Admin Oversight
**Login as**: admin@blackbaud.com (Sarah)
**Action**: View full analytics dashboard
**Result**: See participation stats, rating distributions, engagement metrics

---

## Password Notes

**For Local Development**:
Since this uses Blackbaud OAuth, passwords are managed by Blackbaud's identity system. For local testing:

- Use Blackbaud test accounts
- Or implement a development authentication bypass
- Or generate JWT tokens directly for testing

**Sample JWT Generation** (for testing):
```csharp
// Generate test token for any seeded user email
var tokenService = app.Services.GetRequiredService<IAuthService>();
var token = tokenService.GenerateJwtToken("admin@blackbaud.com", "Admin");
```

---

## Data Relationships

| User | Team | Idea | Comments | Ratings Given |
|------|------|------|----------|---------------|
| Sarah (Admin) | - | - | 0 | 0 |
| Michael (Judge) | - | - | 4 | 9 |
| Jennifer (Judge) | - | - | 2 | 6 |
| David (Judge) | - | - | 3 | 3 |
| Alex | Aqua Innovators | Solar Water System🏆 | 1 | 0 |
| Emma | WindTech | Wind IoT Network🏆 | 1 | 0 |
| Marcus | BioEnergy | Biogas Generator | 1 | 0 |
| Sophia | ColdChain | Cold Storage | 0 | 0 |
| Liam | MobilePower | Renewable Backpack | 0 | 0 |
| Olivia | - | Micro-Hydro | 1 | 0 |
| Noah | GridSmart | AI Energy Mgmt🏆 | 0 | 0 |
| Ava | - | Composting Toilet | 0 | 0 |
| Ethan | Aqua (Member) | - | 0 | 0 |
| Isabella | WindTech (Member) | - | 0 | 0 |

🏆 = Award Winner

---

**Generated**: February 17, 2026  
**Environment**: Demo/Development  
**Hackathon**: Off the Grid 2025
