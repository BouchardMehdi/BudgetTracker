import { BarChart3, ListOrdered, LogIn, LogOut, Menu, PlusCircle, Tags, X } from 'lucide-react';
import { useState } from 'react';
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
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  function closeMenu() {
    setIsMenuOpen(false);
  }

  function handleLogout() {
    logout();
    closeMenu();
  }

  return (
    <header className="navbar">
      <div className="navbar-top">
        <NavLink to="/" className="brand" onClick={closeMenu}>
          <span className="brand-mark">
            <img src="/logo.png" alt="" aria-hidden="true" />
          </span>
          <span>BudgetTracker</span>
        </NavLink>
        <button
          className="menu-toggle"
          type="button"
          onClick={() => setIsMenuOpen((current) => !current)}
          aria-label={isMenuOpen ? 'Fermer le menu' : 'Ouvrir le menu'}
          aria-expanded={isMenuOpen}
          aria-controls="main-navigation"
        >
          {isMenuOpen ? <X size={22} aria-hidden="true" /> : <Menu size={22} aria-hidden="true" />}
        </button>
      </div>
      <nav
        id="main-navigation"
        className={`nav-links ${isMenuOpen ? 'is-open' : ''}`}
        aria-label="Navigation principale"
      >
        {isAuthenticated ? (
          <>
            {links.map(({ to, label, icon: Icon }) => (
              <NavLink key={to} to={to} className="nav-link" onClick={closeMenu}>
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
            <button className="nav-link nav-button" type="button" onClick={handleLogout}>
              <LogOut size={18} aria-hidden="true" />
              <span>Sortir</span>
            </button>
          </>
        ) : (
          <NavLink to="/login" className="nav-link" onClick={closeMenu}>
            <LogIn size={18} aria-hidden="true" />
            <span>Connexion</span>
          </NavLink>
        )}
      </nav>
    </header>
  );
}
