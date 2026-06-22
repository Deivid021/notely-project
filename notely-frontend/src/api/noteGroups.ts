import { api } from './client';

export interface NoteGroup {
  id: string;
  name: string;
  description: string | null;
  noteCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateNoteGroupData {
  name: string;
  description?: string | null;
}

export async function getNoteGroups(): Promise<NoteGroup[]> {
  const { data } = await api.get<NoteGroup[]>('/note-groups');
  return data;
}

export async function createNoteGroup(group: CreateNoteGroupData): Promise<NoteGroup> {
  const { data } = await api.post<NoteGroup>('/note-groups', group);
  return data;
}

export async function deleteNoteGroup(id: string): Promise<void> {
  await api.delete(`/note-groups/${id}`);
}
