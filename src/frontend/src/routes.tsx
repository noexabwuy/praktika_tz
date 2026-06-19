import { createBrowserRouter, Navigate } from 'react-router-dom';
import { ProtectedRoute, RoleRoute, GuestRoute } from './components/ProtectedRoute';
import { MainLayout } from './layouts/MainLayout';

// Импорт страниц
import { LoginPage } from './pages/LoginPage';
import { HomePage } from './pages/HomePage';
import { ApplicationsPage } from './pages/ApplicationsPage';
import { ApplicationCreatePage } from './pages/ApplicationCreatePage';
import { ApplicationDetailsPage } from './pages/ApplicationDetailsPage';
import { DictionariesPage } from './pages/DictionariesPage';
import { StatsPage } from './pages/StatsPage';
import { UsersPage } from './pages/UsersPage';

export const router = createBrowserRouter([
  // Гостевые роуты
  {
    path: '/login',
    element: (
      <GuestRoute>
        <LoginPage />
      </GuestRoute>
    ),
  },

  // Защищенные роуты (с редирект-хабом на корне)
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <HomePage />
      </ProtectedRoute>
    ),
  },
  {
    path: '/applications',
    element: (
      <ProtectedRoute>
        <MainLayout>
          <ApplicationsPage />
        </MainLayout>
      </ProtectedRoute>
    ),
  },
  {
    path: '/applications/new',
    element: (
      <ProtectedRoute>
        <RoleRoute allowedRoles={['Applicant']}>
          <MainLayout>
            <ApplicationCreatePage />
          </MainLayout>
        </RoleRoute>
      </ProtectedRoute>
    ),
  },
  {
    path: '/applications/:id',
    element: (
      <ProtectedRoute>
        <MainLayout>
          <ApplicationDetailsPage />
        </MainLayout>
      </ProtectedRoute>
    ),
  },
  {
    path: '/dictionaries',
    element: (
      <ProtectedRoute>
        <RoleRoute allowedRoles={['Admin']}>
          <MainLayout>
            <DictionariesPage />
          </MainLayout>
        </RoleRoute>
      </ProtectedRoute>
    ),
  },
  {
    path: '/stats',
    element: (
      <ProtectedRoute>
        <RoleRoute allowedRoles={['Director']}>
          <MainLayout>
            <StatsPage />
          </MainLayout>
        </RoleRoute>
      </ProtectedRoute>
    ),
  },
  {
    path: '/users',
    element: (
      <ProtectedRoute>
        <RoleRoute allowedRoles={['Admin', 'Director']}>
          <MainLayout>
            <UsersPage />
          </MainLayout>
        </RoleRoute>
      </ProtectedRoute>
    ),
  },

  // Редирект всех остальных путей
  {
    path: '*',
    element: <Navigate to="/" replace />,
  },
]);
