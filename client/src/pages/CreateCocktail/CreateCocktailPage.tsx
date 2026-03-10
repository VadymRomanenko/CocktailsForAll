import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { IngredientSearchBar } from '../../components/SearchBar/IngredientSearchBar';
import type { SelectedIngredient } from '../../components/SearchBar/IngredientSearchBar';
import { CountryFilter } from '../../components/CountryFilter';
import { createCocktail } from '../../api/cocktails';
import { fetchCountries } from '../../api/countries';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import type { Country } from '../../api/countries';

const schema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  instructions: z.string().min(1, 'Instructions are required'),
  imageUrl: z.string().url().optional().or(z.literal('')),
});

type FormData = z.infer<typeof schema>;

export function CreateCocktailPage() {
  const { t } = useTranslation();
  const [countries, setCountries] = useState<Country[]>([]);
  const [countryId, setCountryId] = useState<number | ''>('');
  const [ingredients, setIngredients] = useState<SelectedIngredient[]>([]);
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    fetchCountries().then(setCountries);
  }, []);

  const { register, handleSubmit, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  if (!isAuthenticated) {
    return (
      <div className="py-6">
        <p className="text-amber-200">{t('create.loginRequired')}</p>
      </div>
    );
  }

  const onSubmit = async (data: FormData) => {
    if (countryId === '' || ingredients.length === 0) {
      setError('Please select a country and at least one ingredient.');
      return;
    }
    setError('');
    try {
      const ingredientsWithIds = ingredients.filter((i): i is { id: number; name: string } => i.id !== null);
      if (ingredientsWithIds.length === 0) {
        setError('Please add at least one ingredient from the list.');
        return;
      }
      const res = await createCocktail({
        name: data.name,
        description: data.description || undefined,
        instructions: data.instructions,
        imageUrl: data.imageUrl || undefined,
        countryId,
        ingredients: ingredientsWithIds.map((i) => ({ ingredientId: i.id, measure: undefined })),
      });
      navigate(`/cocktail/${res.id}`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create cocktail');
    }
  };

  return (
    <div className="py-6 max-w-xl mx-auto">
      <h1 className="text-2xl font-bold text-amber-50 mb-4">{t('create.title')}</h1>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <div className="p-3 rounded-lg bg-red-900/50 text-red-200 text-sm">{error}</div>
        )}
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('create.name')}</label>
          <input
            {...register('name')}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
          />
          {errors.name && <p className="text-red-400 text-sm mt-1">{errors.name.message}</p>}
        </div>
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('create.description')}</label>
          <input
            {...register('description')}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
          />
        </div>
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('create.instructions')}</label>
          <textarea
            {...register('instructions')}
            rows={4}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
          />
          {errors.instructions && <p className="text-red-400 text-sm mt-1">{errors.instructions.message}</p>}
        </div>
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('create.imageUrl')}</label>
          <input
            {...register('imageUrl')}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
          />
        </div>
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('create.country')}</label>
          <CountryFilter countries={countries} value={countryId} onChange={setCountryId} />
        </div>
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('create.ingredients')}</label>
          <IngredientSearchBar value={ingredients} onChange={setIngredients} />
          <p className="text-amber-400/80 text-xs mt-1">{t('create.ingredientsHint')}</p>
        </div>
        <button
          type="submit"
          className="w-full py-2 rounded-xl bg-amber-600 hover:bg-amber-500 text-amber-950 font-semibold"
        >
          Submit for moderation
        </button>
      </form>
    </div>
  );
}
