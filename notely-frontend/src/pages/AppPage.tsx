import { useState, useEffect, useRef } from 'react';
import { useQueryClient, useQuery, useMutation } from '@tanstack/react-query';
import { useAuth } from '../context/AuthContext';
import * as notesApi from '../api/notes';
import * as groupsApi from '../api/noteGroups';

interface SaveArgs {
  id: string;
  title: string;
  content: string;
  noteGroupId: string | null;
}

export default function AppPage() {
  const { logout } = useAuth();
  const qc = useQueryClient();

  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [selectedNoteId, setSelectedNoteId] = useState<string | null>(null);
  const [editTitle, setEditTitle] = useState('');
  const [editContent, setEditContent] = useState('');
  const [editNoteGroupId, setEditNoteGroupId] = useState<string | null>(null);
  const [isCreatingGroup, setIsCreatingGroup] = useState(false);
  const [newGroupName, setNewGroupName] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const groupsQuery = useQuery({
    queryKey: ['groups'],
    queryFn: groupsApi.getNoteGroups,
  });

  const notesQuery = useQuery({
    queryKey: ['notes', selectedGroupId],
    queryFn: () => notesApi.getNotes(selectedGroupId ?? undefined),
  });

  const notes = notesQuery.data ?? [];
  const groups = groupsQuery.data ?? [];
  const selectedNote = notes.find((n) => n.id === selectedNoteId);

  // Backend rejects empty content ([Required]), so we store ​ as placeholder.
  // Normalize for comparison so an "empty" note never shows as dirty on open.
  const normalizeContent = (c: string) => (c === '​' ? '' : c);

  const isDirty =
    selectedNote !== undefined &&
    (editTitle !== selectedNote.title ||
      normalizeContent(editContent) !== normalizeContent(selectedNote.content) ||
      editNoteGroupId !== selectedNote.noteGroupId);

  // Reset selection when group filter changes
  useEffect(() => {
    setSelectedNoteId(null);
  }, [selectedGroupId]);

  // Sync editor fields when switching to a different note
  useEffect(() => {
    if (selectedNote) {
      setEditTitle(selectedNote.title);
      setEditContent(normalizeContent(selectedNote.content));
      setEditNoteGroupId(selectedNote.noteGroupId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedNote?.id]);

  // Auto-resize textarea
  useEffect(() => {
    const el = textareaRef.current;
    if (!el) return;
    el.style.height = 'auto';
    el.style.height = `${el.scrollHeight}px`;
  }, [editContent]);

  // Ctrl+S / Cmd+S to save
  useEffect(() => {
    function onKeyDown(e: KeyboardEvent) {
      if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        e.preventDefault();
        if (selectedNoteId && isDirty) {
          saveNote.mutate({ id: selectedNoteId, title: editTitle, content: editContent, noteGroupId: editNoteGroupId });
        }
      }
    }
    document.addEventListener('keydown', onKeyDown);
    return () => document.removeEventListener('keydown', onKeyDown);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedNoteId, isDirty, editTitle, editContent, editNoteGroupId]);

  const createNote = useMutation({
    mutationFn: () =>
      notesApi.createNote({ title: 'Nova nota', content: '​', noteGroupId: selectedGroupId }),
    onSuccess: (note) => {
      qc.invalidateQueries({ queryKey: ['notes'] });
      qc.invalidateQueries({ queryKey: ['groups'] });
      setSelectedNoteId(note.id);
      setEditTitle(note.title);
      setEditContent(normalizeContent(note.content));
      setEditNoteGroupId(note.noteGroupId);
    },
  });

  const saveNote = useMutation({
    mutationFn: ({ id, title, content, noteGroupId }: SaveArgs) =>
      notesApi.updateNote(id, { title, content: content.trim() || '​', noteGroupId }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['notes'] });
      qc.invalidateQueries({ queryKey: ['groups'] });
    },
  });

  const deleteNote = useMutation({
    mutationFn: (id: string) => notesApi.deleteNote(id),
    onSuccess: (_, deletedId) => {
      const idx = notes.findIndex((n) => n.id === deletedId);
      const next = notes[idx + 1] ?? notes[idx - 1] ?? null;
      setSelectedNoteId(next?.id ?? null);
      qc.invalidateQueries({ queryKey: ['notes'] });
      qc.invalidateQueries({ queryKey: ['groups'] });
    },
  });

  const createGroup = useMutation({
    mutationFn: (name: string) => groupsApi.createNoteGroup({ name }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['groups'] });
      setNewGroupName('');
      setIsCreatingGroup(false);
    },
  });

  const deleteGroup = useMutation({
    mutationFn: (id: string) => groupsApi.deleteNoteGroup(id),
    onSuccess: (_, id) => {
      qc.invalidateQueries({ queryKey: ['groups'] });
      qc.invalidateQueries({ queryKey: ['notes'] });
      if (selectedGroupId === id) setSelectedGroupId(null);
    },
  });

  function handleSelectNote(id: string) {
    if (isDirty && selectedNoteId) {
      saveNote.mutate({ id: selectedNoteId, title: editTitle, content: editContent, noteGroupId: editNoteGroupId });
    }
    setSelectedNoteId(id);
  }

  function handleSave() {
    if (selectedNoteId && isDirty) {
      saveNote.mutate({ id: selectedNoteId, title: editTitle, content: editContent, noteGroupId: editNoteGroupId });
    }
  }

  function handleCreateGroup(e: React.FormEvent) {
    e.preventDefault();
    if (newGroupName.trim()) createGroup.mutate(newGroupName.trim());
  }

  function formatDate(iso: string) {
    return new Date(iso).toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });
  }

  const totalNoteCount = groups.reduce((sum, g) => sum + g.noteCount, 0);

  return (
    <div className="flex flex-col h-screen bg-white text-gray-900">
      {/* Header */}
      <header className="flex items-center justify-between px-5 h-12 border-b border-gray-200 shrink-0">
        <span className="font-semibold text-gray-800">Notely</span>
        <button
          onClick={logout}
          className="text-sm text-gray-400 hover:text-gray-700 transition-colors"
        >
          Sair
        </button>
      </header>

      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar */}
        <aside className="w-56 flex flex-col border-r border-gray-200 shrink-0 overflow-y-auto bg-gray-50">
          <div className="p-3 pt-4">
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide px-3 mb-2">
              Grupos
            </p>

            {/* All notes */}
            <button
              onClick={() => setSelectedGroupId(null)}
              className={`w-full flex items-center justify-between px-3 py-2 rounded-lg text-sm mb-0.5 transition-colors ${
                selectedGroupId === null
                  ? 'bg-indigo-100 text-indigo-700 font-medium'
                  : 'text-gray-600 hover:bg-gray-200'
              }`}
            >
              <span>Todas as notas</span>
              <span className="text-xs font-normal text-gray-400">{totalNoteCount}</span>
            </button>

            {/* Groups */}
            {groups.map((group) => (
              <div
                key={group.id}
                onClick={() => setSelectedGroupId(group.id)}
                className={`group/item w-full flex items-center justify-between px-3 py-2 rounded-lg text-sm mb-0.5 cursor-pointer transition-colors ${
                  selectedGroupId === group.id
                    ? 'bg-indigo-100 text-indigo-700 font-medium'
                    : 'text-gray-600 hover:bg-gray-200'
                }`}
              >
                <span className="truncate flex-1 min-w-0">{group.name}</span>
                <div className="flex items-center gap-1.5 shrink-0 ml-1">
                  <span className="text-xs font-normal text-gray-400">{group.noteCount}</span>
                  <button
                    onClick={(e) => {
                      e.stopPropagation();
                      if (window.confirm(`Excluir grupo "${group.name}"?\nAs notas não serão excluídas.`)) {
                        deleteGroup.mutate(group.id);
                      }
                    }}
                    className="opacity-0 group-hover/item:opacity-100 text-gray-400 hover:text-red-500 transition-all"
                    title="Excluir grupo"
                  >
                    <TrashIcon />
                  </button>
                </div>
              </div>
            ))}

            {/* New group input / button */}
            <div className="mt-1">
              {isCreatingGroup ? (
                <form onSubmit={handleCreateGroup}>
                  <input
                    autoFocus
                    value={newGroupName}
                    onChange={(e) => setNewGroupName(e.target.value)}
                    onBlur={() => {
                      if (!newGroupName.trim()) setIsCreatingGroup(false);
                    }}
                    onKeyDown={(e) => {
                      if (e.key === 'Escape') {
                        setIsCreatingGroup(false);
                        setNewGroupName('');
                      }
                    }}
                    placeholder="Nome do grupo"
                    className="w-full px-3 py-1.5 text-sm rounded-lg border border-indigo-300 outline-none focus:ring-2 focus:ring-indigo-200 bg-white"
                  />
                </form>
              ) : (
                <button
                  onClick={() => setIsCreatingGroup(true)}
                  className="w-full flex items-center gap-1.5 px-3 py-2 text-sm text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  <span className="text-base leading-none font-light">+</span>
                  <span>Novo grupo</span>
                </button>
              )}
            </div>
          </div>
        </aside>

        {/* Notes list */}
        <div className="w-72 flex flex-col border-r border-gray-200 shrink-0 overflow-hidden">
          <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 shrink-0">
            <span className="text-sm font-medium text-gray-700 truncate">
              {selectedGroupId
                ? (groups.find((g) => g.id === selectedGroupId)?.name ?? 'Grupo')
                : 'Todas as notas'}
            </span>
            <button
              onClick={() => createNote.mutate()}
              disabled={createNote.isPending}
              className="flex items-center gap-0.5 text-sm text-indigo-600 hover:text-indigo-800 font-medium transition-colors disabled:opacity-50 shrink-0 ml-2"
            >
              <span className="text-base leading-none">+</span>&nbsp;Nova
            </button>
          </div>

          <div className="flex-1 overflow-y-auto">
            {notesQuery.isLoading && (
              <p className="text-sm text-gray-400 text-center mt-10">Carregando...</p>
            )}
            {notesQuery.isSuccess && notes.length === 0 && (
              <p className="text-sm text-gray-400 text-center mt-10">Nenhuma nota ainda.</p>
            )}
            {notes.map((note) => (
              <div
                key={note.id}
                onClick={() => handleSelectNote(note.id)}
                className={`px-4 py-3 cursor-pointer border-b border-gray-100 transition-colors ${
                  selectedNoteId === note.id
                    ? 'bg-indigo-50 border-l-2 border-l-indigo-500'
                    : 'hover:bg-gray-50 border-l-2 border-l-transparent'
                }`}
              >
                <p className="text-sm font-medium text-gray-800 truncate">
                  {note.title || 'Sem título'}
                </p>
                <p className="text-xs text-gray-400 mt-0.5 truncate">
                  {note.content || 'Sem conteúdo'}
                </p>
                <p className="text-xs text-gray-300 mt-1">{formatDate(note.updatedAt)}</p>
              </div>
            ))}
          </div>
        </div>

        {/* Editor */}
        <div className="flex-1 flex flex-col overflow-hidden min-w-0">
          {!selectedNote ? (
            <div className="flex-1 flex items-center justify-center select-none">
              <p className="text-sm text-gray-300">Selecione ou crie uma nota</p>
            </div>
          ) : (
            <>
              {/* Title */}
              <div className="px-8 pt-7 pb-3 shrink-0">
                <input
                  value={editTitle}
                  onChange={(e) => setEditTitle(e.target.value)}
                  placeholder="Título"
                  className="w-full text-2xl font-semibold text-gray-900 outline-none placeholder:text-gray-300 bg-transparent"
                />
              </div>

              {/* Toolbar */}
              <div className="flex items-center gap-3 px-8 py-2 border-b border-gray-100 shrink-0">
                <label className="text-xs text-gray-400 shrink-0">Grupo</label>
                <select
                  value={editNoteGroupId ?? ''}
                  onChange={(e) => setEditNoteGroupId(e.target.value || null)}
                  className="text-xs text-gray-600 border border-gray-200 rounded px-2 py-1 outline-none focus:border-indigo-400 bg-white"
                >
                  <option value="">Sem grupo</option>
                  {groups.map((g) => (
                    <option key={g.id} value={g.id}>
                      {g.name}
                    </option>
                  ))}
                </select>

                <div className="flex-1" />

                {saveNote.isError && (
                  <span className="text-xs text-red-500">Erro ao salvar</span>
                )}
                {isDirty && !saveNote.isError && (
                  <span className="text-xs text-gray-400">Não salvo</span>
                )}

                <button
                  onClick={handleSave}
                  disabled={saveNote.isPending || !isDirty}
                  className="text-xs px-3 py-1 rounded bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-40 transition-colors"
                >
                  {saveNote.isPending ? 'Salvando…' : 'Salvar'}
                </button>

                <button
                  onClick={() => {
                    if (window.confirm('Excluir esta nota?')) {
                      deleteNote.mutate(selectedNoteId!);
                    }
                  }}
                  disabled={deleteNote.isPending}
                  className="text-xs px-3 py-1 rounded border border-gray-200 text-gray-500 hover:text-red-500 hover:border-red-300 disabled:opacity-40 transition-colors"
                >
                  Excluir
                </button>
              </div>

              {/* Content */}
              <div className="flex-1 overflow-y-auto px-8 py-5">
                <textarea
                  ref={textareaRef}
                  value={editContent}
                  onChange={(e) => setEditContent(e.target.value)}
                  placeholder="Comece a escrever…"
                  className="w-full text-sm text-gray-700 outline-none resize-none leading-relaxed placeholder:text-gray-300 bg-transparent"
                  style={{ minHeight: '100%' }}
                />
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

function TrashIcon() {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="13"
      height="13"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <polyline points="3 6 5 6 21 6" />
      <path d="M19 6l-1 14a2 2 0 01-2 2H8a2 2 0 01-2-2L5 6" />
      <path d="M10 11v6" />
      <path d="M14 11v6" />
      <path d="M9 6V4a1 1 0 011-1h4a1 1 0 011 1v2" />
    </svg>
  );
}
