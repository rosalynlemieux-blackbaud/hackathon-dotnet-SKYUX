export interface User {
  id: number;
  blackbaudId: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  user: User;
}

export interface Hackathon {
  id: number;
  name: string;
  description: string;
  status: 'upcoming' | 'active' | 'judging' | 'completed';
  registrationStart: Date;
  registrationEnd: Date;
  startDate: Date;
  endDate: Date;
  judgingStart: Date;
  judgingEnd: Date;
  winnersAnnouncement: Date;
  rules?: string;
  faq?: string;
  maxTeamSize: number;
  allowLateSubmissions: boolean;
  isPublic: boolean;
  tracks: Track[];
  awards: Award[];
  judgingCriteria: JudgingCriterion[];
  milestones: Milestone[];
}

export interface Track {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  color?: string;
  displayOrder: number;
}

export interface Award {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  icon?: string;
  displayOrder: number;
}

export interface JudgingCriterion {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  weight: number;
  maxScore: number;
  displayOrder: number;
}

export interface Milestone {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  dueDate: Date;
  isComplete: boolean;
  displayOrder: number;
}

export interface Team {
  id: number;
  hackathonId: number;
  name: string;
  description?: string;
  imageUrl?: string;
  leaderId: number;
  leader?: User;
  isLookingForMembers: boolean;
  teamMembers: TeamMember[];
  createdAt: Date;
}

export interface TeamMember {
  id: number;
  teamId: number;
  userId: number;
  user?: User;
  joinedAt: Date;
}

export interface Idea {
  id: number;
  hackathonId: number;
  teamId: number;
  team?: Team;
  trackId?: number;
  track?: Track;
  submittedBy: number;
  submittedByUser?: User;
  author?: User;
  title: string;
  description: string;
  status: 'draft' | 'submitted' | 'under_review' | 'winner';
  imageUrl?: string;
  demoUrl?: string;
  repositoryUrl?: string;
  videoUrl?: string;
  technicalDetails?: string;
  ratings?: Rating[];
  comments?: Comment[];
  awards?: Award[];
  submission?: {
    teamId?: number;
  };
  ideaAwards?: IdeaAward[];
  createdAt: Date;
  submittedAt?: Date;
}

export interface IdeaAward {
  id: number;
  ideaId: number;
  awardId: number;
  award?: Award;
  awardedAt: Date;
}

export interface Rating {
  id: number;
  ideaId: number;
  judgeId: number;
  judge?: User;
  criterionId: number;
  criterion?: JudgingCriterion;
  score: number;
  feedback?: string;
  createdAt: Date;
}

export interface Comment {
  id: number;
  ideaId: number;
  userId: number;
  user?: User;
  content: string;
  parentCommentId?: number;
  parentComment?: Comment;
  replies?: Comment[];
  isDeleted: boolean;
  createdAt: Date;
}
