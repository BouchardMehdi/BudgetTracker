import { Plus, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import { categoriesApi } from '../api/categoriesApi';
import { getApiErrorMessage } from '../utils/apiError';

export default function Categories() {
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({ name: '', type: 'expense' });
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});

  async function loadCategories() {
    try {
      const data = await categoriesApi.getAll();
      setCategories(data);
    } catch (loadError) {
      setError(getApiErrorMessage(loadError));
    }
  }

  useEffect(() => {
    loadCategories();
  }, []);

  function updateField(event) {
    const { name, value } = event.target;
    setFieldErrors((current) => ({ ...current, [name]: '' }));
    setForm((current) => ({ ...current, [name]: value }));
  }

  async function createCategory(event) {
    event.preventDefault();
    setError('');

    if (!form.name.trim()) {
      setFieldErrors({ name: 'Le nom est obligatoire.' });
      return;
    }

    try {
      const created = await categoriesApi.create({ name: form.name.trim(), type: form.type });
      setCategories((current) => [...current, created].sort((a, b) => a.name.localeCompare(b.name)));
      setForm({ name: '', type: 'expense' });
      setFieldErrors({});
    } catch (createError) {
      setError(getApiErrorMessage(createError));
    }
  }

  async function deleteCategory(category) {
    const confirmed = window.confirm(`Supprimer la categorie "${category.name}" ?`);
    if (!confirmed) {
      return;
    }

    setError('');
    try {
      await categoriesApi.remove(category.id);
      setCategories((current) => current.filter((item) => item.id !== category.id));
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  }

  return (
    <section className="page-stack">
      <div className="page-header">
        <div>
          <p className="eyebrow">Organisation</p>
          <h1>Categories</h1>
        </div>
      </div>

      <form className="inline-form" onSubmit={createCategory}>
        <label>
          Nom
          <input name="name" value={form.name} onChange={updateField} placeholder="Nom de categorie" />
          {fieldErrors.name && <span className="field-error">{fieldErrors.name}</span>}
        </label>
        <select name="type" value={form.type} onChange={updateField}>
          <option value="expense">Depense</option>
          <option value="income">Revenu</option>
        </select>
        <button className="primary-button" type="submit">
          <Plus size={18} aria-hidden="true" />
          Ajouter
        </button>
      </form>

      {error && <div className="alert error">{error}</div>}

      <div className="category-grid">
        {categories.map((category) => (
          <article className="category-card" key={category.id}>
            <div>
              <strong>{category.name}</strong>
              <span className={`pill ${category.type}`}>{category.type === 'income' ? 'Revenu' : 'Depense'}</span>
            </div>
            <button
              className="icon-button danger"
              type="button"
              onClick={() => deleteCategory(category)}
              title="Supprimer"
              aria-label={`Supprimer ${category.name}`}
            >
              <Trash2 size={18} aria-hidden="true" />
            </button>
          </article>
        ))}
      </div>
    </section>
  );
}
