import { BarChart3, ListOrdered, LogIn, LogOut, PlusCircle, Tags } from 'lucide-react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { currencyOptions, useCurrency } from '../context/CurrencyContext';

const links = [
  { to: '/', label: 'Dashboard', icon: BarChart3 },
  { to: '/transactions', label: 'Transactions', icon: ListOrdered },
  { to: '/transactions/add', label: 'Ajouter', icon: PlusCircle },
  { to: '/categories', label: 'Categories', icon: Tags },
];

export default function Navbar() {
  const { isAuthenticated, user, logout } = useAuth();
  const { currency, setCurrency } = useCurrency();

  return (
    <header className="navbar">
      <NavLink to="/" className="brand">
        <span className="brand-mark">BT</span>
        <span>BudgetTracker</span>
      </NavLink>
      <nav className="nav-links" aria-label="Navigation principale">
        {isAuthenticated ? (
          <>
            {links.map(({ to, label, icon: Icon }) => (
              <NavLink key={to} to={to} className="nav-link">
                <Icon size={18} aria-hidden="true" />
                <span>{label}</span>
              </NavLink>
            ))}
            <select
              className="currency-select"
              value={currency}
              onChange={(event) => setCurrency(event.target.value)}
              aria-label="Devise"
            >
              {currencyOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            <span className="user-chip">{user?.username}</span>
            <button className="nav-link nav-button" type="button" onClick={logout}>
              <LogOut size={18} aria-hidden="true" />
              <span>Sortir</span>
            </button>
          </>
        ) : (
          <NavLink to="/login" className="nav-link">
            <LogIn size={18} aria-hidden="true" />
            <span>Connexion</span>
          </NavLink>
        )}
      </nav>
    </header>
  );
}
