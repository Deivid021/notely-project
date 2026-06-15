import { createBrowserRouter, Navigate } from 'react-router-dom';
import { PrivateRoute, PublicRoute } from './components/PrivateRoute';
import LoginPage from './pages/LoginPage';
import AppPage from './pages/AppPage';

export const router = createBrowserRouter([
  {
    element: <PublicRoute />,
    children: [{ path: '/login', element: <LoginPage /> }],
  },
  {
    element: <PrivateRoute />,
    children: [{ path: '/', element: <AppPage /> }],
  },
  { path: '*', element: <Navigate to="/" replace /> },
]);
