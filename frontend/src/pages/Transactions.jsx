import { Plus } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { transactionsApi } from '../api/transactionsApi';
import TransactionList from '../components/TransactionList';

export default function Transactions() {
  const [transactions, setTransactions] = useState([]);
  const [isLoading, setIsLoading] = useState(true);

  async function loadTransactions() {
    setIsLoading(true);
    try {
      const data = await transactionsApi.getAll();
      setTransactions(data);
    } finally {
      setIsLoading(false);
    }
  }

  async function deleteTransaction(id) {
    await transactionsApi.remove(id);
    setTransactions((current) => current.filter((transaction) => transaction.id !== id));
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

      {isLoading ? (
        <div className="empty-state">Chargement...</div>
      ) : (
        <TransactionList transactions={transactions} onDelete={deleteTransaction} />
      )}
    </section>
  );
}
