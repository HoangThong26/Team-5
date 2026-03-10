export interface UserProfile {
  userId: number;
  email: string;
  fullName: string;
  phone?: string;
  avatarUrl?: string;
  status?: string;
  roleName?: string;
}

export interface UpdateProfileRequest {
  fullName: string; 
  phone?: string;
  avatarUrl?: string;
}

export interface UpdatePasswordProfileRequest {
  oldPassword: string;
  newPassword: string;
  otp: string;
}

export interface AdminCreateUserRequest {
  email: string;
  fullName: string;
  phone: string;
  role: string;
  password: string;
}
