# Off the Grid 2025 - Demo Data Seeding

## Overview

The database seeder automatically populates the Hackathon Platform with realistic demo data for the "Off the Grid 2025" hackathon. This provides an instant working environment for demos, testing, and development.

## Configuration

### Enable/Disable Seeding

In `appsettings.json`:

```json
{
  "Database": {
    "SeedOnStartup": true
  }
}
```

- **`true`**: Seeds database on application startup (if not already seeded)
- **`false`**: Skips seeding (production default)

### Seeding Behavior

- **Idempotent**: Safe to run multiple times - checks if data exists before seeding
- **Automatic**: Runs automatically when `SeedOnStartup` is `true`
- **Migration Friendly**: Applies pending migrations before seeding

## Seeded Data Summary

### Users (14 Total)

#### 1 Administrator
- **Email**: admin@blackbaud.com
- **Name**: Sarah Administrator
- **Role**: Admin

#### 3 Judges
- **Michael Chen** (judge1@blackbaud.com)
- **Jennifer Rodriguez** (judge2@blackbaud.com)
- **David Patel** (judge3@blackbaud.com)

#### 10 Participants
- Alex Johnson (alex.johnson@blackbaud.com)
- Emma Williams (emma.williams@blackbaud.com)
- Marcus Thompson (marcus.thompson@blackbaud.com)
- Sophia Garcia (sophia.garcia@blackbaud.com)
- Liam Martinez (liam.martinez@blackbaud.com)
- Olivia Anderson (olivia.anderson@blackbaud.com)
- Noah Thomas (noah.thomas@blackbaud.com)
- Ava Jackson (ava.jackson@blackbaud.com)
- Ethan White (ethan.white@blackbaud.com)
- Isabella Harris (isabella.harris@blackbaud.com)

### Hackathon

**Off the Grid 2025**
- **Theme**: Sustainable Off-Grid Solutions
- **Status**: Active
- **Duration**: 28 days (currently active)
- **Judging Period**: 7 days (upcoming)
- **Max Team Size**: 5
- **Description**: Innovation challenge focused on sustainability, renewable energy, and off-grid solutions for communities to thrive independently

### Ideas (8 Total)

#### 🏆 Award Winners

1. **Solar-Powered Water Purification System** ⭐ Best Overall ($5,000)
   - Team: Aqua Innovators
   - Author: Alex Johnson
   - Features: UV sterilization, 1000L/day capacity, mobile monitoring
   - Status: Submitted
   - Ratings: 9, 8, 9 (Avg: 8.7)

2. **Wind-Powered IoT Mesh Network** ⭐ Innovation Award ($3,000)
   - Team: WindTech Collective
   - Author: Emma Williams
   - Features: 50km range, disaster recovery, LoRa mesh
   - Status: Submitted
   - Ratings: 8, 9, 8 (Avg: 8.3)

3. **AI-Powered Energy Management for Microgrids** ⭐ Technical Excellence ($2,500)
   - Team: GridSmart AI
   - Author: Noah Thomas
   - Features: 30% waste reduction, predictive load balancing
   - Status: Submitted
   - Ratings: 9, 8 (Avg: 8.5)

#### Other Submitted Ideas

4. **Biogas Generator for Rural Kitchens**
   - Team: BioEnergy Squad
   - Author: Marcus Thompson
   - Features: 2m³ digester, produces 2hrs cooking gas/day
   - Ratings: 7, 8, 7 (Avg: 7.3)

5. **Off-Grid Cold Storage for Farmers**
   - Team: ColdChain Champions
   - Author: Sophia Garcia
   - Features: 500L capacity, 7-day battery backup, IoT monitoring
   - Ratings: 9, 8 (Avg: 8.5)

6. **Portable Renewable Energy Backpack**
   - Team: MobilePower Pioneers
   - Author: Liam Martinez
   - Features: 60W solar, 300Wh battery, weighs 3kg
   - Ratings: 7, 8 (Avg: 7.5)

7. **Micro-Hydro Power for Mountain Communities**
   - Author: Olivia Anderson (solo)
   - Features: 1-5kW capacity, works with low flow rates
   - Ratings: 8, 9 (Avg: 8.5)

8. **Composting Toilet with Biochar Production**
   - Author: Ava Jackson (solo)
   - Features: Waterless, produces 5kg biochar/month
   - Status: Draft (not yet submitted)

### Teams (6 Total)

1. **Aqua Innovators** (2 members)
   - Focus: Clean water solutions
   - Leader: Alex Johnson

2. **WindTech Collective** (2 members)
   - Focus: Wind power connectivity
   - Leader: Emma Williams

3. **BioEnergy Squad** (1 member)
   - Focus: Waste to energy
   - Leader: Marcus Thompson

4. **ColdChain Champions** (1 member)
   - Focus: Food/medicine preservation
   - Leader: Sophia Garcia

5. **MobilePower Pioneers** (1 member)
   - Focus: Portable renewable energy
   - Leader: Liam Martinez

6. **GridSmart AI** (1 member)
   - Focus: Intelligent energy management
   - Leader: Noah Thomas

### Comments (11 Total)

Comments distributed across all submitted ideas:
- 2 comments on Solar Water Purification
- 2 comments on Wind-Powered IoT
- 1 comment on Biogas Generator
- 2 comments on Cold Storage
- 1 comment on Renewable Backpack
- 1 comment on Micro-Hydro
- 2 comments on AI Energy Management

Comments include:
- Judge feedback and questions
- Peer praise and suggestions
- Technical discussions

### Ratings (18 Total)

All submitted ideas have been rated by 2-3 judges:
- **Scale**: 1-10
- **Average Scores**: Range from 7.3 to 8.7
- **Feedback**: Each rating includes detailed written feedback

### Awards (3 Total)

- **Best Overall Solution**: $5,000 - Solar Water Purification
- **Innovation Award**: $3,000 - Wind-Powered IoT
- **Technical Excellence Award**: $2,500 - AI Energy Management

**Total Prize Money**: $10,500

## Data Relationships

```
Hackathon (1)
├── Users (14)
│   ├── Admin (1)
│   ├── Judges (3)
│   └── Participants (10)
├── Ideas (8)
│   ├── Submitted (7)
│   └── Draft (1)
├── Teams (6)
│   └── TeamMembers (8)
├── Comments (11)
├── Ratings (18)
└── Awards (3)
```

## Use Cases

### 1. Demo Walkthrough
Login as admin or judge to see:
- Active hackathon with multiple submissions
- Diverse ideas with ratings and comments
- Winners already announced
- Full judging history

### 2. Testing
Use seeded data to test:
- Idea submission workflows
- Team management
- Commenting and discussion
- Rating systems
- Winner selection
- Email notifications
- File uploads (add attachments to existing ideas)

### 3. Development
Bootstrap new features with realistic data:
- Analytics dashboard (populated with real metrics)
- Search and filtering (diverse idea topics)
- User management (mix of roles)
- Notification systems (pre-existing events)

## Seeder Implementation

### Location
`/backend/src/Blackbaud.Hackathon.Platform.Service/DataAccess/DbSeeder.cs`

### Methods
- `SeedAsync()`: Main orchestration method
- `SeedUsers()`: Creates admin, judges, and participants
- `SeedHackathon()`: Creates Off the Grid 2025 hackathon
- `SeedIdeas()`: Creates 8 diverse ideas with descriptions
- `SeedTeams()`: Creates teams and assigns members
- `SeedComments()`: Creates discussion threads
- `SeedRatings()`: Creates judge ratings with feedback
- `SeedAwards()`: Creates winner awards

### Features
- **Realistic Data**: Names, emails, descriptions based on actual hackathon patterns
- **Temporal Distribution**: Timestamps spread over realistic time periods
- **Relationships**: All foreign keys properly linked
- **Idempotent**: Checks for existing data before seeding
- **Logged**: Progress logged at INFO level

## Manual Seeding

If you need to manually trigger seeding:

1. Enable in appsettings.json:
   ```json
   {
     "Database": {
       "SeedOnStartup": true
     }
   }
   ```

2. Restart the application

3. Check logs for:
   ```
   Starting database seeding for Off the Grid 2025...
   Seeded 14 users
   Seeded Off the Grid 2025 hackathon
   Seeded 8 ideas
   Seeded 6 teams with 8 members
   Seeded 11 comments
   Seeded 18 ratings
   Seeded 3 awards
   Database seeding completed successfully!
   ```

## Clearing Seeded Data

To start fresh:

1. Drop and recreate database:
   ```bash
   dotnet ef database drop --force
   dotnet ef database update
   ```

2. Restart application with `SeedOnStartup: true`

## Production Recommendations

**IMPORTANT**: Always set `SeedOnStartup: false` in production!

Seeding is intended for:
- ✅ Development environments
- ✅ Demo environments
- ✅ Testing/QA
- ❌ Production (use real data)

## Customization

To modify seeded data, edit `DbSeeder.cs`:

- Add more users in `SeedUsers()`
- Change hackathon details in `SeedHackathon()`
- Add/modify ideas in `SeedIdeas()`
- Adjust ratings in `SeedRatings()`
- Change award amounts in `SeedAwards()`

Remember to keep data realistic and maintain referential integrity!

---

**Last Updated**: February 17, 2026  
**Hackathon**: Off the Grid 2025  
**Total Entities**: 60+ seeded records
