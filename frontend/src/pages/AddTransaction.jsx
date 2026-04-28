import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { categoriesApi } from '../api/categoriesApi';
import { transactionsApi } from '../api/transactionsApi';
import TransactionForm from '../components/TransactionForm';
import { getApiErrorMessage } from '../utils/apiError';

export default function AddTransaction() {
  const [categories, setCategories] = useState([]);
  const [loadError, setLoadError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    async function loadCategories() {
      try {
        const data = await categoriesApi.getAll();
        setCategories(data);
      } catch (error) {
        setLoadError(getApiErrorMessage(error));
      }
    }

    loadCategories();
  }, []);

  async function createTransaction(payload) {
    setIsSubmitting(true);
    try {
      await transactionsApi.create(payload);
      navigate('/transactions');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page-stack">
      <div className="page-header">
        <div>
          <p className="eyebrow">Nouvelle operation</p>
          <h1>Ajouter une transaction</h1>
        </div>
      </div>

      <TransactionForm categories={categories} onSubmit={createTransaction} isSubmitting={isSubmitting} />
      {loadError && <div className="alert error">{loadError}</div>}
    </section>
  );
}
