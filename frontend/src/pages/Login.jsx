import { LogIn } from 'lucide-react';
import { useState } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Login() {
  const { isAuthenticated, login } = useAuth();
  const [form, setForm] = useState({ identifier: '', password: '' });
  const [error, setError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const navigate = useNavigate();

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  function updateField(event) {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');
    setIsSubmitting(true);

    try {
      await login(form.identifier, form.password);
      navigate('/');
    } catch (err) {
      setError(err.response?.data || 'Identifiants invalides.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="auth-page">
      <form className="auth-panel" onSubmit={handleSubmit}>
        <div>
          <p className="eyebrow">Bienvenue</p>
          <h1>Connexion</h1>
        </div>

        {error && <div className="alert error">{error}</div>}

        <label>
          Email ou username
          <input name="identifier" value={form.identifier} onChange={updateField} autoComplete="username" />
        </label>

        <label>
          Mot de passe
          <input name="password" type="password" value={form.password} onChange={updateField} autoComplete="current-password" />
        </label>

        <button className="primary-button" type="submit" disabled={isSubmitting}>
          <LogIn size={18} aria-hidden="true" />
          {isSubmitting ? 'Connexion...' : 'Se connecter'}
        </button>

        <p className="auth-switch">
          Pas encore de compte ? <Link to="/register">Creer un compte</Link>
        </p>
      </form>
    </section>
  );
}
