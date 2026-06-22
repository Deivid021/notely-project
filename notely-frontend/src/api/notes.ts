import { api } from './client';

export interface Note {
  id: string;
  title: string;
  content: string;
  noteGroupId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateNoteData {
  title: string;
  content: string;
  noteGroupId?: string | null;
}

export interface UpdateNoteData {
  title: string;
  content: string;
  noteGroupId: string | null;
}

export async function getNotes(groupId?: string): Promise<Note[]> {
  const { data } = await api.get<Note[]>('/notes', {
    params: groupId ? { groupId } : undefined,
  });
  return data;
}

export async function createNote(note: CreateNoteData): Promise<Note> {
  const { data } = await api.post<Note>('/notes', note);
  return data;
}

export async function updateNote(id: string, note: UpdateNoteData): Promise<Note> {
  const { data } = await api.put<Note>(`/notes/${id}`, note);
  return data;
}

export async function deleteNote(id: string): Promise<void> {
  await api.delete(`/notes/${id}`);
}
