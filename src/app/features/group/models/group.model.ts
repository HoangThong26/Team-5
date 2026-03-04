export interface CreateGroupRequest {
  groupName: string;
  targetMembers: number;
}

export interface CreateGroupResponse {
  message: string;
  groupId: number;
}

export interface Topic {
  title: string;
  description: string;
}

export interface Member {
  fullName: string;
  email: string;
}
