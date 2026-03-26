export interface DefenseEvaluationRequest {
  defenseId: number;
  councilMemberUserId: number;
  presentationScore: number;
  demoScore: number;
  qaScore: number;
  finalScore: number;
  comment?: string;
}

export interface DefenseScoreDto {
  score: number;
  comment?: string;
}