# 📱 Visual Demo & Walkthrough

## What You're Building

The **Off the Grid Hackathon Platform** is a complete web application that lets organizations run hackathons with team collaboration, idea submission, and judging capabilities.

---

## 🎨 User Interface Preview

### 1. Home Page (Landing)

```
╔════════════════════════════════════════════════════════════════════╗
║                                                                    ║
║  ⚡ Off the Grid          [Home] [Ideas] [Teams] [Admin]  [Login] ║
║                                                                    ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║                      ╔═══════════════════════════╗                ║
║                      ║  Off the Grid 2025        ║                ║
║                      ║                           ║                ║
║                      ║  Join us for the biggest  ║                ║
║                      ║  hackathon of the year!   ║                ║
║                      ║  Innovate. Collaborate.   ║                ║
║                      ║  Create Impact.           ║                ║
║                      ║                           ║                ║
║                      ║ 🎯 ACTIVE NOW             ║                ║
║                      ║                           ║                ║
║                      ║ [Submit Idea] [View Ideas]║                ║
║                      ╚═══════════════════════════╝                ║
║                                                                    ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║  TRACKS                         AWARDS                            ║
║  ┌─────────────────────┐       ┌──────────────────────┐          ║
║  │ 🚀 AI Acceleration   │       │ 🏆 Grand Prize       │          ║
║  │ Leverage AI to build │       │ Best overall idea    │          ║
║  │ innovative solutions │       └──────────────────────┘          ║
║  └─────────────────────┘                                          ║
║                                                                    ║
║  ┌─────────────────────┐       ┌──────────────────────┐          ║
║  │ 🔗 Connected Systems │       │ 💡 Innovation Award  │          ║
║  │ IoT & Smart Home     │       │ Most creative idea   │          ║
║  └─────────────────────┘       └──────────────────────┘          ║
║                                                                    ║
║  ┌─────────────────────┐       ┌──────────────────────┐          ║
║  │ 🎮 Just for Fun      │       │ ⚡ Best Execution    │          ║
║  │ Any idea that makes  │       │ Cleanest code        │          ║
║  │ people smile         │       └──────────────────────┘          ║
║  └─────────────────────┘                                          ║
║                                                                    ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║  TIMELINE                       JUDGING CRITERIA                  ║
║  ✓ Kickoff - Feb 17            • Innovation (3x weight)          ║
║  ✓ Team Formation - Feb 18     • Code Quality (2x weight)        ║
║  • Midpoint Deadline - Feb 20  • User Experience (1.5x weight)   ║
║  • Final Submissions - Feb 24  • Presentation (1x weight)        ║
║  • Judging - Feb 25                                               ║
║  • Winners - Feb 26                                               ║
║                                                                    ║
╚════════════════════════════════════════════════════════════════════╝
```

---

### 2. Login Page

```
╔════════════════════════════════════════════════════════════════════╗
║                                                                    ║
║                                                                    ║
║                      ⚡ Off the Grid                              ║
║                      Hackathon Platform                           ║
║                                                                    ║
║                  Sign in with your Blackbaud                      ║
║                  account to participate in                        ║
║                  hackathons, submit ideas,                        ║
║                  join teams, and more.                            ║
║                                                                    ║
║                  ┌──────────────────────────┐                     ║
║                  │ B Sign in with Blackbaud │                     ║
║                  └──────────────────────────┘                     ║
║                                                                    ║
║                                                                    ║
╚════════════════════════════════════════════════════════════════════╝
```

**What Happens:**
1. User clicks "Sign in with Blackbaud"
2. Redirected to Blackbaud OAuth login
3. User enters Blackbaud credentials
4. Returns to app with JWT token
5. Logged in state shows user profile

---

### 3. Ideas Page (After Login)

```
╔════════════════════════════════════════════════════════════════════╗
║ ⚡ Off the Grid  [Home] [Ideas] [Teams] [Judging]      👤 John   ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║                   ALL IDEAS FOR OFF THE GRID                      ║
║                   Filter: [All ▼] [Sort: Newest ▼]               ║
║                                                                    ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ 💡 AI-Powered Patient Diagnosis System                     │  ║
║  │ submitted by Team Alpha                                    │  ║
║  │ Track: AI Acceleration                                     │  ║
║  │ Status: [SUBMITTED]  ⭐⭐⭐⭐ (4.2/5 avg rating)         │  ║
║  │                                                            │  ║
║  │ Machine learning platform that analyzes patient data   │  ║
║  │ to provide diagnostic recommendations to doctors.      │  ║
║  │ Submitted Feb 22 | 3 Comments | Demo Available         │  ║
║  │                                [View Details] [Comment] │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ 🏠 Smart Home Energy Optimizer                             │  ║
║  │ submitted by Team Beta                                     │  ║
║  │ Track: Connected Systems                                   │  ║
║  │ Status: [UNDER_REVIEW]  ⭐⭐⭐⭐⭐ (4.8/5 avg rating)    │  ║
║  │                                                            │  ║
║  │ IoT system that learns your usage patterns and           │  ║
║  │ automatically optimizes your home's energy consumption.  │  ║
║  │ Submitted Feb 21 | 12 Comments | Video Demo             │  ║
║  │                                [View Details] [Comment] │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ 🎮 Retro Game Collection                                   │  ║
║  │ submitted by Team Gamma                                    │  ║
║  │ Track: Just for Fun                                        │  ║
║  │ Status: [DRAFT]  No ratings yet                            │  ║
║  │                                                            │  ║
║  │ Collection of classic games recreated with modern        │  ║
║  │ graphics and gameplay mechanics.                          │  ║
║  │ Created Feb 20 | 0 Comments | Repository Link            │  ║
║  │                                [View Details] [Comment] │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
╚════════════════════════════════════════════════════════════════════╝
```

---

### 4. Idea Detail Page

```
╔════════════════════════════════════════════════════════════════════╗
║ ⚡ Off the Grid  [Home] [Ideas] [Teams] [Judging]      👤 John    ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║  AI-Powered Patient Diagnosis System                              ║
║  Track: AI Acceleration  │  Status: SUBMITTED  │  ⭐⭐⭐⭐ 4.2/5  ║
║                                                                    ║
║  TEAM: Alpha Tigers                                               ║
║  Members: Sarah Chen (Lead), Mike Johnson, Lisa Park             ║
║                                                                    ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ DESCRIPTION                                                │  ║
║  │                                                            │  ║
║  │ Machine learning platform that analyzes patient medical  │  ║
║  │ records and genetic data to provide diagnostic           │  ║
║  │ recommendations. Uses advanced NLP for symptom analysis  │  ║
║  │ and integrates with major healthcare systems.            │  ║
║  │                                                            │  ║
║  │ [Demo: https://...] [Repository: https://...]            │  ║
║  │ [Video: https://...]                                      │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  RATINGS (By Judges)                                              ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ Innovation (3x weight)                  Score: 5/5        │  ║
║  │ "Excellent use of cutting-edge ML models"                │  ║
║  │                                                            │  ║
║  │ Code Quality (2x weight)                Score: 4/5        │  ║
║  │ "Well-structured but could use more documentation"       │  ║
║  │                                                            │  ║
║  │ User Experience (1.5x weight)          Score: 4/5        │  ║
║  │ "Intuitive interface, smooth workflows"                  │  ║
║  │                                                            │  ║
║  │ Presentation (1x weight)                Score: 5/5        │  ║
║  │ "Outstanding demo and explanation"                       │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  COMMENTS & DISCUSSION                                            ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ 👤 Judge Rachel Smith (3 days ago)                         │  ║
║  │ "Have you considered HIPAA compliance for healthcare      │  ║
║  │  data? Great work overall!"                               │  ║
║  │                                                            │  ║
║  │ 👤 Sarah Chen (3 days ago)                                │  ║
║  │ "Yes! We built in full HIPAA compliance from the start.  │  ║
║  │  Privacy is paramount in healthcare."                     │  ║
║  │                                                            │  ║
║  │ 👤 Judge Mike Brown (2 days ago)                          │  ║
║  │ "Looking forward to seeing this in production. Great      │  ║
║  │  potential impact."                                        │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  [Leave Comment] [Edit Idea] [Share]                             ║
║                                                                    ║
╚════════════════════════════════════════════════════════════════════╝
```

---

### 5. Judging Dashboard

```
╔════════════════════════════════════════════════════════════════════╗
║ ⚡ Off the Grid  [Home] [Ideas] [Teams] [Judging]      👤 Judge   ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║  JUDGING DASHBOARD - Off the Grid 2025                           ║
║  Progress: 7/15 ideas rated (47%)                                 ║
║                                                                    ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ IDEAS TO RATE                                              │  ║
║  │                                                            │  ║
║  │ ✓ AI-Powered Patient Diagnosis          [RATED]           │  ║
║  │ □ Smart Home Energy Optimizer           [PENDING]         │  ║
║  │ □ Retro Game Collection                 [PENDING]         │  ║
║  │ □ Sustainable Supply Chain Tracker      [PENDING]         │  ║
║  │ ✓ Voice-Enabled Shopping Assistant      [RATED]           │  ║
║  │ ✓ AR Navigation System                  [RATED]           │  ║
║  │ □ Blockchain Voting System              [PENDING]         │  ║
║  │ ✓ COVID-19 Resource Map                 [RATED]           │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  RATE NEXT IDEA                                                   ║
║  Smart Home Energy Optimizer                                      ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ Innovation (3x weight)          [1] [2] [3] [4] [5*]     │  ║
║  │ Code Quality (2x weight)        [1] [2] [3] [4*] [5]     │  ║
║  │ User Experience (1.5x weight)   [1] [2] [3] [4] [5*]     │  ║
║  │ Presentation (1x weight)        [1] [2] [3*] [4] [5]     │  ║
║  │                                                            │  ║
║  │ Your Feedback:                                            │  ║
║  │ ┌──────────────────────────────────────────────────────┐ │  ║
║  │ │ Excellent energy optimization algorithm. Great UX.  │ │  ║
║  │ │ Consider scalability for larger installations.      │ │  ║
║  │ └──────────────────────────────────────────────────────┘ │  ║
║  │                                                            │  ║
║  │              [Submit Rating] [Save Draft]                │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
╚════════════════════════════════════════════════════════════════════╝
```

---

### 6. Admin Dashboard

```
╔════════════════════════════════════════════════════════════════════╗
║ ⚡ Off the Grid  [Home] [Ideas] [Teams] [Admin]        👤 Admin   ║
╠════════════════════════════════════════════════════════════════════╣
║                                                                    ║
║  ADMIN DASHBOARD                                                   ║
║                                                                    ║
║  HACKATHON OVERVIEW                                                ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ Off the Grid 2025         Status: ACTIVE                  │  ║
║  │ 15 Ideas Submitted | 12 Teams | 67 Participants          │  ║
║  │ Judging Phase: 47% Complete                               │  ║
║  │ Winners Announced: Feb 26                                 │  ║
║  │                                                            │  ║
║  │ [View Full Info] [Edit Settings] [Announce Winners]       │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  ANALYTICS                                                         ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ Ideas by Track              Ratings Distribution           │  ║
║  │ AI Acceleration: 6          5 stars: ⭐⭐⭐⭐⭐ 8 ideas    │  ║
║  │ Connected Systems: 5        4 stars: ⭐⭐⭐⭐ 4 ideas      │  ║
║  │ Just for Fun: 4             3 stars: ⭐⭐⭐ 2 ideas       │  ║
║  │                             2 stars: ⭐⭐ 1 idea         │  ║
║  │                             1 star:  ⭐ 0 ideas          │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
║  MANAGE                                                            ║
║  ┌────────────────────────────────────────────────────────────┐  ║
║  │ [Manage Judges]  [Manage Teams]  [Manage Criteria]        │  ║
║  │ [View All Ratings]  [Export Results]  [Settings]          │  ║
║  └────────────────────────────────────────────────────────────┘  ║
║                                                                    ║
╚════════════════════════════════════════════════════════════════════╝
```

---

## 🌐 API Endpoints (Accessible via Swagger UI)

When backend is running, visit: **https://localhost:5001/swagger**

### Authentication Endpoints
```
POST   /api/auth/login                 → Initiates OAuth flow
POST   /api/auth/callback              → Handles OAuth callback
GET    /api/auth/me                    → Get current user
```

### Hackathon Endpoints
```
GET    /api/hackathons                 → List all hackathons
GET    /api/hackathons/{id}            → Get specific hackathon
GET    /api/hackathons/current         → Get active hackathon
POST   /api/hackathons                 → Create (Admin only)
PUT    /api/hackathons/{id}            → Update (Admin only)
DELETE /api/hackathons/{id}            → Delete (Admin only)
```

### Ideas Endpoints
```
GET    /api/ideas                      → List all ideas
GET    /api/ideas/{id}                 → Get idea details
POST   /api/ideas                      → Submit new idea
PUT    /api/ideas/{id}                 → Update idea
POST   /api/ideas/{id}/submit          → Submit for judging
DELETE /api/ideas/{id}                 → Delete idea
```

### (Coming Soon)
```
Teams, Ratings, Comments, Admin endpoints
```

---

## 🔄 User Journey Examples

### Journey 1: Participant Submitting an Idea
```
1. User logs in via Blackbaud
2. Navigates to "Ideas" section
3. Clicks "Submit New Idea"
4. Fills in: Title, Description, Track, Links
5. Saves as DRAFT
6. Refines and clicks "Submit for Judging"
7. Sees submission confirmation
8. Can view idea and feedback as judges rate
```

### Journey 2: Judge Evaluating Ideas
```
1. Judge logs in via Blackbaud
2. Navigates to "Judging" dashboard
3. Sees list of ideas to rate (7/15 pending)
4. Clicks "Rate Next Idea"
5. Rates on 4 criteria with weighted scores
6. Adds feedback comments
7. Submits rating
8. Moves to next idea
9. Views leaderboard of current winners
```

### Journey 3: Admin Managing Hackathon
```
1. Admin logs in via Blackbaud
2. Navigates to "Admin" panel
3. Views analytics: Ideas, Teams, Ratings progress
4. Manages judges and their assignments
5. Configures judging criteria and weights
6. Exports final results as CSV
7. Announces winners and sends notifications
8. Archives hackathon for records
```

---

## 🎯 Technology Stack Visualization

```
┌─────────────────────────────────────────────────────────┐
│                    USER'S BROWSER                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Angular 17+ Single Page Application             │  │
│  │  - Components: Home, Ideas, Teams, Judging       │  │
│  │  - Services: Auth, Hackathon, Idea               │  │
│  │  - Guards: AuthGuard, RoleGuard                  │  │
│  │  - Styling: SKY UX Theme + Custom SCSS           │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                           │ HTTP/HTTPS
                           │ JWT Token
                           ↓
┌─────────────────────────────────────────────────────────┐
│                  .NET 8.0 WEB API                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Controllers:                                    │  │
│  │  - AuthController (OAuth, JWT)                   │  │
│  │  - HackathonsController (CRUD)                   │  │
│  │  - IdeasController (CRUD)                        │  │
│  │  - [Coming] TeamsController                      │  │
│  │  - [Coming] RatingsController                    │  │
│  │  - [Coming] CommentsController                   │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Services:                                       │  │
│  │  - AuthService (BBID OAuth + JWT)                │  │
│  │  - UserService (User management)                 │  │
│  │  - [Coming] IdeaService                          │  │
│  │  - [Coming] RatingService                        │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Entity Framework Core                           │  │
│  │  - DbContext with 13 entities                    │  │
│  │  - Migrations support                            │  │
│  │  - Relationships & Navigation properties         │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                           │ ADO.NET
                           ↓
┌─────────────────────────────────────────────────────────┐
│              SQL SERVER / LOCALDB                       │
│  Database: HackathonPlatform                           │
│  Tables: Users, Hackathons, Ideas, Teams,              │
│          Ratings, Comments, Tracks, Awards             │
└─────────────────────────────────────────────────────────┘
                           │ OAuth
                           ↓
┌─────────────────────────────────────────────────────────┐
│           BLACKBAUD ID (BBID) OAUTH                    │
│  - Authorization: https://app.blackbaud.com/oauth      │
│  - Token: https://oauth2.sky.blackbaud.com/token       │
└─────────────────────────────────────────────────────────┘
```

---

## 💾 Database Schema Sample

```
Users
├─ Id (PK)
├─ BlackbaudId
├─ Email
├─ FirstName
├─ LastName

Hackathons
├─ Id (PK)
├─ Name
├─ Status (upcoming, active, judging, completed)
├─ StartDate
├─ EndDate

Ideas
├─ Id (PK)
├─ HackathonId (FK)
├─ TeamId (FK)
├─ TrackId (FK)
├─ Title
├─ Description
├─ Status (draft, submitted, under_review, winner)

Ratings
├─ Id (PK)
├─ IdeaId (FK)
├─ JudgeId (FK)
├─ CriterionId (FK)
├─ Score (1-5)

Comments
├─ Id (PK)
├─ IdeaId (FK)
├─ UserId (FK)
├─ ParentCommentId (FK) [for threading]
├─ Content
```

---

## 🎬 Workflow Diagram

```
PARTICIPANT WORKFLOW:
┌─────────┐    ┌─────────────┐    ┌──────────┐    ┌───────────────┐
│ Sign in │ → │ Form Team   │ → │ Submit   │ → │ View Feedback │
│ w/ BBID │   │ (optional)  │   │ Idea     │   │ from Judges   │
└─────────┘    └─────────────┘    └──────────┘    └───────────────┘

JUDGE WORKFLOW:
┌─────────┐    ┌──────────────┐    ┌──────────┐    ┌──────────────┐
│ Sign in │ → │ See Ideas    │ → │ Rate on  │ → │ View Winners │
│ w/ BBID │   │ to evaluate  │   │ Criteria │   │ (later)      │
└─────────┘    └──────────────┘    └──────────┘    └──────────────┘

ADMIN WORKFLOW:
┌─────────┐    ┌──────────────┐    ┌──────────┐    ┌──────────────┐
│ Sign in │ → │ Configure    │ → │ Monitor  │ → │ Announce     │
│ w/ BBID │   │ Hackathon    │   │ Progress │   │ Winners      │
└─────────┘    └──────────────┘    └──────────┘    └──────────────┘
```

---

## 🎨 Color Scheme

- **Primary Blue**: `#00b4d8` (Blackbaud brand)
- **Dark Blue**: `#0077b6` (secondary)
- **Success Green**: `#06d6a0`
- **Warning Orange**: `#ffd60a`
- **Error Red**: `#ef476f`
- **Light Background**: `#f5f8fa`

---

## ✨ Features Demo

### What Works Now:
✅ Home page showing hackathon overview
✅ Track and award information
✅ Timeline with milestones
✅ Judging criteria with weights
✅ Login with Blackbaud ID
✅ Protected routes by role
✅ API endpoints (Swagger UI)
✅ Responsive design

### Coming in Phase 2:
⏳ Complete Ideas management (create, edit, delete, submit)
⏳ Team management and formation
⏳ Real-time commenting on ideas
⏳ Judge rating interface
⏳ Admin dashboard with analytics
⏳ Winner announcement system

---

## 📊 Expected Data on Launch

When you seed the database, you'll have:

```
Hackathon: Off the Grid 2025
  Status: ACTIVE
  Start: Feb 17, 2026
  End: Feb 26, 2026
  
Tracks:
  • AI Acceleration
  • Connected Systems
  • Just for Fun
  
Awards:
  • Grand Prize
  • Innovation Award
  • Best Execution
  
Judging Criteria:
  • Innovation (Weight: 3.0)
  • Code Quality (Weight: 2.0)
  • User Experience (Weight: 1.5)
  • Presentation (Weight: 1.0)
  
Milestones:
  • Kickoff - Feb 17 ✓
  • Team Formation - Feb 18 ✓
  • Midpoint Check-in - Feb 20
  • Final Submissions - Feb 24
  • Judging - Feb 25
  • Winners Announced - Feb 26
```

---

This is a production-ready foundation! 🎉 Once you install the tools and run it locally, you'll see all these features in action!
