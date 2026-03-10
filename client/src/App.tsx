import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { MainLayout } from './components/Layout/MainLayout';
import { HomePage } from './pages/Home/HomePage';
import { SearchPage } from './pages/Search/SearchPage';
import { CocktailDetailPage } from './pages/CocktailDetail/CocktailDetailPage';
import { FavoritesPage } from './pages/Favorites/FavoritesPage';
import { LoginPage } from './pages/Login/LoginPage';
import { RegisterPage } from './pages/Register/RegisterPage';
import { ProfilePage } from './pages/Profile/ProfilePage';
import { CreateCocktailPage } from './pages/CreateCocktail/CreateCocktailPage';
import { AdminPage } from './pages/Admin/AdminPage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <MainLayout>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/search" element={<SearchPage />} />
            <Route path="/cocktail/:id" element={<CocktailDetailPage />} />
            <Route path="/favorites" element={<FavoritesPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/create" element={<CreateCocktailPage />} />
            <Route path="/admin" element={<AdminPage />} />
          </Routes>
        </MainLayout>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
