import { BarChart3, ListOrdered, PlusCircle, Tags } from 'lucide-react';
import { NavLink } from 'react-router-dom';

const links = [
  { to: '/', label: 'Dashboard', icon: BarChart3 },
  { to: '/transactions', label: 'Transactions', icon: ListOrdered },
  { to: '/transactions/add', label: 'Ajouter', icon: PlusCircle },
  { to: '/categories', label: 'Categories', icon: Tags },
];

export default function Navbar() {
  return (
    <header className="navbar">
      <NavLink to="/" className="brand">
        <span className="brand-mark">BT</span>
        <span>BudgetTracker</span>
      </NavLink>
      <nav className="nav-links" aria-label="Navigation principale">
        {links.map(({ to, label, icon: Icon }) => (
          <NavLink key={to} to={to} className="nav-link">
            <Icon size={18} aria-hidden="true" />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>
    </header>
  );
}
