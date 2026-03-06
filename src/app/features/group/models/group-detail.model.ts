export interface GroupDetail {
  groupId: number;
  groupName: string;
  status?: string;
  createdAt?: string;
  leaderName?: string;
  topic?: Topic;
  members: GroupMember[];
  pendingInvitations: PendingInvitation[];
}
export interface PendingInvitation {
  invitationId: number;
  inviteeEmail: string;
  inviteeName: string;
  sentAt: string;
}
export interface Topic {
  title: string;
  description?: string;
}

export interface GroupMember {
  userId: number;
  roleInGroup?: string;
  joinedAt?: string;
}


