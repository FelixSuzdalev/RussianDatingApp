using RussianDatingApp.Model;

using System.Collections.ObjectModel;
using System.Linq;

namespace RussianDatingApp.ViewModel
{
    public class FilterViewModel : ABaseViewModel
    {
        private readonly RussianDatingAppEntities _dbContext;

        private ObservableCollection<AgeCategory> _ageCategories;
        public ObservableCollection<AgeCategory> AgeCategories
        {
            get => _ageCategories;
            set => SetPropertyChanged(ref _ageCategories, value);
        }

        private ObservableCollection<City> _cities;
        public ObservableCollection<City> Cities
        {
            get => _cities;
            set => SetPropertyChanged(ref _cities, value);
        }

        private ObservableCollection<ZodiacSign> _zodiacSigns;
        public ObservableCollection<ZodiacSign> ZodiacSigns
        {
            get => _zodiacSigns;
            set => SetPropertyChanged(ref _zodiacSigns, value);
        }

        private AgeCategory _selectedAgeCategory;
        public AgeCategory SelectedAgeCategory
        {
            get => _selectedAgeCategory;
            set => SetPropertyChanged(ref _selectedAgeCategory, value);
        }

        // Добавьте другие свойства для фильтров

        public FilterViewModel()
        {
            _dbContext = new RussianDatingAppEntities();
            LoadFilters();
        }

        private void LoadFilters()
        {
            AgeCategories = new ObservableCollection<AgeCategory>(_dbContext.AgeCategory.ToList());
            Cities = new ObservableCollection<City>(_dbContext.City.ToList());
            ZodiacSigns = new ObservableCollection<ZodiacSign>(_dbContext.ZodiacSign.ToList());
        }
    }
}