import { Link } from 'react-router-dom';
import { Heart } from 'lucide-react';

interface CocktailCardProps {
  id: number;
  name: string;
  imageUrl: string | null;
  countryName: string;
  isFavorite: boolean;
  onToggleFavorite?: (id: number) => void;
}

export function CocktailCard({ id, name, imageUrl, countryName, isFavorite, onToggleFavorite }: CocktailCardProps) {
  return (
    <Link
      to={`/cocktail/${id}`}
      className="block rounded-2xl overflow-hidden bg-amber-900/50 border border-amber-700/50 hover:border-amber-600/60 transition-colors"
    >
      <div className="aspect-[4/3] relative bg-amber-950">
        {imageUrl ? (
          <img
            src={imageUrl}
            alt={name}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-amber-600 text-4xl">🍸</div>
        )}
        {onToggleFavorite && (
          <button
            type="button"
            onClick={(e) => {
              e.preventDefault();
              onToggleFavorite(id);
            }}
            className="absolute top-2 right-2 p-2 rounded-full bg-amber-950/80 hover:bg-amber-900/90 transition-colors"
            aria-label={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
          >
            <Heart
              size={20}
              className={isFavorite ? 'fill-amber-400 text-amber-400' : 'text-amber-200'}
            />
          </button>
        )}
      </div>
      <div className="p-4">
        <h3 className="font-semibold text-amber-50 truncate">{name}</h3>
        <p className="text-sm text-amber-300/80">{countryName}</p>
      </div>
    </Link>
  );
}
