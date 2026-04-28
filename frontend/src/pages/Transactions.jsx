import { Plus, RotateCcw } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { categoriesApi } from '../api/categoriesApi';
import { transactionsApi } from '../api/transactionsApi';
import TransactionList from '../components/TransactionList';
import { getApiErrorMessage } from '../utils/apiError';

export default function Transactions() {
  const [transactions, setTransactions] = useState([]);
  const [categories, setCategories] = useState([]);
  const [filters, setFilters] = useState({ search: '', type: 'all', categoryId: 'all', month: '' });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const monthOptions = useMemo(() => {
    const months = new Set(transactions.map((transaction) => transaction.transactionDate.slice(0, 7)));
    return Array.from(months).sort().reverse();
  }, [transactions]);

  const filteredTransactions = useMemo(() => {
    const search = filters.search.trim().toLowerCase();

    return transactions.filter((transaction) => {
      const matchesSearch =
        !search ||
        transaction.title.toLowerCase().includes(search) ||
        transaction.description?.toLowerCase().includes(search);
      const matchesType = filters.type === 'all' || transaction.type === filters.type;
      const matchesCategory = filters.categoryId === 'all' || transaction.categoryId === Number(filters.categoryId);
      const matchesMonth = !filters.month || transaction.transactionDate.slice(0, 7) === filters.month;

      return matchesSearch && matchesType && matchesCategory && matchesMonth;
    });
  }, [filters, transactions]);

  async function loadTransactions() {
    setIsLoading(true);
    setError('');
    try {
      const [transactionData, categoryData] = await Promise.all([
        transactionsApi.getAll(),
        categoriesApi.getAll(),
      ]);
      setTransactions(transactionData);
      setCategories(categoryData);
    } catch (loadError) {
      setError(getApiErrorMessage(loadError));
    } finally {
      setIsLoading(false);
    }
  }

  function updateFilter(event) {
    const { name, value } = event.target;
    setFilters((current) => ({ ...current, [name]: value }));
  }

  function resetFilters() {
    setFilters({ search: '', type: 'all', categoryId: 'all', month: '' });
  }

  async function deleteTransaction(transaction) {
    const confirmed = window.confirm(`Supprimer la transaction "${transaction.title}" ?`);
    if (!confirmed) {
      return;
    }

    setError('');

    try {
      await transactionsApi.remove(transaction.id);
      setTransactions((current) => current.filter((item) => item.id !== transaction.id));
    } catch (deleteError) {
      setError(getApiErrorMessage(deleteError));
    }
  }

  useEffect(() => {
    loadTransactions();
  }, []);

  return (
    <section className="page-stack">
      <div className="page-header">
        <div>
          <p className="eyebrow">Historique</p>
          <h1>Transactions</h1>
        </div>
        <Link className="primary-button" to="/transactions/add">
          <Plus size={18} aria-hidden="true" />
          Ajouter
        </Link>
      </div>

      <div className="filters-panel">
        <label>
          Recherche
          <input
            name="search"
            value={filters.search}
            onChange={updateFilter}
            placeholder="Titre ou description"
          />
        </label>

        <label>
          Type
          <select name="type" value={filters.type} onChange={updateFilter}>
            <option value="all">Tous</option>
            <option value="income">Revenus</option>
            <option value="expense">Depenses</option>
          </select>
        </label>

        <label>
          Categorie
          <select name="categoryId" value={filters.categoryId} onChange={updateFilter}>
            <option value="all">Toutes</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </label>

        <label>
          Mois
          <select name="month" value={filters.month} onChange={updateFilter}>
            <option value="">Tous</option>
            {monthOptions.map((month) => (
              <option key={month} value={month}>
                {month}
              </option>
            ))}
          </select>
        </label>

        <button className="secondary-button" type="button" onClick={resetFilters}>
          <RotateCcw size={18} aria-hidden="true" />
          Reinitialiser
        </button>
      </div>

      {error && <div className="alert error">{error}</div>}

      {isLoading ? (
        <div className="empty-state">Chargement...</div>
      ) : (
        <TransactionList transactions={filteredTransactions} onDelete={deleteTransaction} />
      )}
    </section>
  );
}
