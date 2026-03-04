export interface GroupDetail {
  groupId: number;
  groupName: string;
  status?: string;
  createdAt?: string;
  leaderName?: string;
  topic?: Topic;
  members: GroupMember[];
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


