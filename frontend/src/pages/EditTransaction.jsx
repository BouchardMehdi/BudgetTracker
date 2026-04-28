import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { categoriesApi } from '../api/categoriesApi';
import { transactionsApi } from '../api/transactionsApi';
import TransactionForm from '../components/TransactionForm';
import { getApiErrorMessage } from '../utils/apiError';

export default function EditTransaction() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [transaction, setTransaction] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [loadError, setLoadError] = useState('');

  useEffect(() => {
    async function loadData() {
      setIsLoading(true);
      setLoadError('');

      try {
        const [categoryData, transactionData] = await Promise.all([
          categoriesApi.getAll(),
          transactionsApi.getById(id),
        ]);
        setCategories(categoryData);
        setTransaction(transactionData);
      } catch (error) {
        setLoadError(getApiErrorMessage(error));
      } finally {
        setIsLoading(false);
      }
    }

    loadData();
  }, [id]);

  async function updateTransaction(payload) {
    setIsSubmitting(true);
    try {
      await transactionsApi.update(id, payload);
      navigate('/transactions');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page-stack">
      <div className="page-header">
        <div>
          <p className="eyebrow">Mise a jour</p>
          <h1>Modifier la transaction</h1>
        </div>
      </div>

      {loadError && <div className="alert error">{loadError}</div>}
      {isLoading ? (
        <div className="empty-state">Chargement...</div>
      ) : (
        transaction && (
          <TransactionForm
            categories={categories}
            initialValues={transaction}
            onSubmit={updateTransaction}
            isSubmitting={isSubmitting}
            submitLabel="Mettre a jour"
            resetOnSuccess={false}
          />
        )
      )}
    </section>
  );
}
