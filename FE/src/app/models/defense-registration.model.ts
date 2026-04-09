export interface DefenseRegistrationStatusDto {
  groupId: number;
  groupName: string;
  evaluatedWeeks: number;
  passedWeeks: number;
  passRate: number;
  isEligibleForDefense: boolean;
  isRegistered: boolean;
}

export interface DefenseRegistrationItemDto {
  defenseId: number;
  groupId: number;
  groupName: string;
  status: string;
  councilId?: number;
  councilName?: string;
  room?: string;
  startTime?: string;
  endTime?: string;
}

export interface DefenseCommitteeMemberDto {
  userId: number;
  fullName: string;
}

export interface DefenseCommitteeDto {
  councilId: number;
  councilName: string;
  members: DefenseCommitteeMemberDto[];
}

export interface CouncilUserDto {
  userId: number;
  fullName: string;
  councilId: number;
  councilName: string;
}

export interface CreateDefenseScheduleRequest {
  defenseId: number;
  councilId: number;
  room: string;
  startTime: string;
  endTime?: string | null;
}

export interface UpdateDefenseScheduleRequest {
  councilId: number;
  room: string;
  startTime: string;
  endTime?: string | null;
}
