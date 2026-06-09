import { useAuth } from '../context/AuthContext';

export default function AppPage() {
  const { logout } = useAuth();

  return (
    <div className="flex h-screen items-center justify-center bg-gray-50">
      <div className="text-center">
        <h2 className="text-xl font-semibold text-gray-800">App — em construção</h2>
        <p className="mt-1 text-sm text-gray-500">Autenticação funcionando.</p>
        <button
          onClick={logout}
          className="mt-4 rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-100"
        >
          Sair
        </button>
      </div>
    </div>
  );
}
