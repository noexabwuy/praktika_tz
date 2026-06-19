import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const HomePage: React.FC = () => {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  const getRedirectPath = () => {
    switch (user.role) {
      case 'Admin':
        return '/dictionaries';
      case 'Director':
        return '/stats';
      case 'Applicant':
      case 'Manager':
      default:
        return '/applications';
    }
  };

  return <Navigate to={getRedirectPath()} replace />;
};

