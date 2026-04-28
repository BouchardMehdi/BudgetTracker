import { ChevronLeft, ChevronRight, Plus, RotateCcw } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { categoriesApi } from '../api/categoriesApi';
import { transactionsApi } from '../api/transactionsApi';
import TransactionList from '../components/TransactionList';
import { getApiErrorMessage } from '../utils/apiError';

const initialFilters = { search: '', type: '', categoryId: '', month: '' };

function buildMonthOptions() {
  const options = [];
  const today = new Date();

  for (let index = 0; index < 18; index += 1) {
    const date = new Date(today.getFullYear(), today.getMonth() - index, 1);
    const value = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
    options.push(value);
  }

  return options;
}

export default function Transactions() {
  const [transactions, setTransactions] = useState([]);
  const [categories, setCategories] = useState([]);
  const [filters, setFilters] = useState(initialFilters);
  const [pagination, setPagination] = useState({ page: 1, pageSize: 10, totalItems: 0, totalPages: 0 });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const monthOptions = useMemo(() => buildMonthOptions(), []);

  async function loadTransactions(nextPage = pagination.page) {
    setIsLoading(true);
    setError('');

    try {
      const params = {
        page: nextPage,
        pageSize: pagination.pageSize,
        search: filters.search || undefined,
        type: filters.type || undefined,
        categoryId: filters.categoryId || undefined,
        month: filters.month || undefined,
      };

      const data = await transactionsApi.getAll(params);
      setTransactions(data.items);
      setPagination({
        page: data.page,
        pageSize: data.pageSize,
        totalItems: data.totalItems,
        totalPages: data.totalPages,
      });
    } catch (loadError) {
      setError(getApiErrorMessage(loadError));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    async function loadCategories() {
      try {
        const categoryData = await categoriesApi.getAll();
        setCategories(categoryData);
      } catch (loadError) {
        setError(getApiErrorMessage(loadError));
      }
    }

    loadCategories();
  }, []);

  useEffect(() => {
    loadTransactions(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filters, pagination.pageSize]);

  function updateFilter(event) {
    const { name, value } = event.target;
    setFilters((current) => ({ ...current, [name]: value }));
  }

  function updatePageSize(event) {
    setPagination((current) => ({ ...current, pageSize: Number(event.target.value), page: 1 }));
  }

  function resetFilters() {
    setFilters(initialFilters);
  }

  async function deleteTransaction(transaction) {
    const confirmed = window.confirm(`Supprimer la transaction "${transaction.title}" ?`);
    if (!confirmed) {
      return;
    }

    setError('');

    try {
      await transactionsApi.remove(transaction.id);
      loadTransactions(pagination.page);
    } catch (deleteError) {
      setError(getApiErrorMessage(deleteError));
    }
  }

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
            <option value="">Tous</option>
            <option value="income">Revenus</option>
            <option value="expense">Depenses</option>
          </select>
        </label>

        <label>
          Categorie
          <select name="categoryId" value={filters.categoryId} onChange={updateFilter}>
            <option value="">Toutes</option>
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
        <>
          <TransactionList transactions={transactions} onDelete={deleteTransaction} />
          <div className="pagination-bar">
            <span>
              {pagination.totalItems} transaction{pagination.totalItems > 1 ? 's' : ''}
            </span>
            <label>
              Par page
              <select value={pagination.pageSize} onChange={updatePageSize}>
                <option value="5">5</option>
                <option value="10">10</option>
                <option value="20">20</option>
                <option value="50">50</option>
              </select>
            </label>
            <div className="pagination-actions">
              <button
                className="icon-button"
                type="button"
                onClick={() => loadTransactions(pagination.page - 1)}
                disabled={pagination.page <= 1}
                title="Page precedente"
                aria-label="Page precedente"
              >
                <ChevronLeft size={18} aria-hidden="true" />
              </button>
              <strong>
                {pagination.totalPages === 0 ? 0 : pagination.page} / {pagination.totalPages}
              </strong>
              <button
                className="icon-button"
                type="button"
                onClick={() => loadTransactions(pagination.page + 1)}
                disabled={pagination.page >= pagination.totalPages}
                title="Page suivante"
                aria-label="Page suivante"
              >
                <ChevronRight size={18} aria-hidden="true" />
              </button>
            </div>
          </div>
        </>
      )}
    </section>
  );
}
