import { UserPlus } from 'lucide-react';
import { useState } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Register() {
  const { isAuthenticated, register } = useAuth();
  const [form, setForm] = useState({ username: '', email: '', password: '' });
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

    if (form.password.length < 8) {
      setError('Le mot de passe doit contenir au moins 8 caracteres.');
      return;
    }

    setIsSubmitting(true);

    try {
      await register(form.username, form.email, form.password);
      navigate('/');
    } catch (err) {
      setError(err.response?.data || 'Impossible de creer le compte.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="auth-page">
      <form className="auth-panel" onSubmit={handleSubmit}>
        <div>
          <p className="eyebrow">Nouveau compte</p>
          <h1>Inscription</h1>
        </div>

        {error && <div className="alert error">{error}</div>}

        <label>
          Username
          <input name="username" value={form.username} onChange={updateField} autoComplete="username" />
        </label>

        <label>
          Email
          <input name="email" type="email" value={form.email} onChange={updateField} autoComplete="email" />
        </label>

        <label>
          Mot de passe
          <input name="password" type="password" value={form.password} onChange={updateField} autoComplete="new-password" />
        </label>

        <button className="primary-button" type="submit" disabled={isSubmitting}>
          <UserPlus size={18} aria-hidden="true" />
          {isSubmitting ? 'Creation...' : 'Creer le compte'}
        </button>

        <p className="auth-switch">
          Deja un compte ? <Link to="/login">Se connecter</Link>
        </p>
      </form>
    </section>
  );
}
