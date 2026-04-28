import { Save } from 'lucide-react';
import { useMemo, useState } from 'react';

const initialForm = {
  title: '',
  amount: '',
  type: 'expense',
  transactionDate: new Date().toISOString().slice(0, 10),
  description: '',
  categoryId: '',
};

export default function TransactionForm({ categories, onSubmit, isSubmitting = false }) {
  const [form, setForm] = useState(initialForm);
  const [error, setError] = useState('');

  const availableCategories = useMemo(
    () => categories.filter((category) => category.type === form.type),
    [categories, form.type]
  );

  function updateField(event) {
    const { name, value } = event.target;
    setForm((current) => ({
      ...current,
      [name]: value,
      ...(name === 'type' ? { categoryId: '' } : {}),
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');

    if (!form.title.trim()) {
      setError('Le titre est obligatoire.');
      return;
    }

    if (Number(form.amount) <= 0) {
      setError('Le montant doit etre superieur a 0.');
      return;
    }

    if (!form.categoryId) {
      setError('Choisis une categorie.');
      return;
    }

    await onSubmit({
      title: form.title.trim(),
      amount: Number(form.amount),
      type: form.type,
      transactionDate: form.transactionDate,
      description: form.description.trim(),
      categoryId: Number(form.categoryId),
    });

    setForm(initialForm);
  }

  return (
    <form className="form-panel" onSubmit={handleSubmit}>
      {error && <div className="alert error">{error}</div>}

      <div className="form-grid">
        <label>
          Titre
          <input name="title" value={form.title} onChange={updateField} placeholder="Salaire, courses..." />
        </label>

        <label>
          Montant
          <input name="amount" type="number" min="0.01" step="0.01" value={form.amount} onChange={updateField} />
        </label>

        <label>
          Type
          <select name="type" value={form.type} onChange={updateField}>
            <option value="expense">Depense</option>
            <option value="income">Revenu</option>
          </select>
        </label>

        <label>
          Date
          <input name="transactionDate" type="date" value={form.transactionDate} onChange={updateField} />
        </label>

        <label>
          Categorie
          <select name="categoryId" value={form.categoryId} onChange={updateField}>
            <option value="">Selectionner</option>
            {availableCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </label>

        <label className="full-width">
          Description
          <textarea name="description" value={form.description} onChange={updateField} rows="4" />
        </label>
      </div>

      <button className="primary-button" type="submit" disabled={isSubmitting}>
        <Save size={18} aria-hidden="true" />
        {isSubmitting ? 'Enregistrement...' : 'Enregistrer'}
      </button>
    </form>
  );
}
