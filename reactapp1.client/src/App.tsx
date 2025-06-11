import { useEffect, useState } from 'react';
import './App.css';

interface Forecast {
    id?: number;
    date: string;
    temperatureC: number;
    temperatureF?: number;
    summary: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>([]);
    const [newForecast, setNewForecast] = useState<Forecast>({
        date: '',
        temperatureC: 0,
        summary: '',
    });

    useEffect(() => {
        populateWeatherData();
    }, []);

    const populateWeatherData = async () => {
        try {
            const response = await fetch('/api/weatherforecast');
            if (!response.ok) throw new Error('Failed to fetch');
            const data = await response.json();
            setForecasts(data);
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setNewForecast({ ...newForecast, [name]: name === 'temperatureC' ? parseInt(value) : value });
    };

    const addForecast = async () => {
        try {
            const response = await fetch('/api/weatherforecast', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newForecast),
            });
            if (!response.ok) throw new Error('Failed to post');
            await populateWeatherData();
            setNewForecast({ date: '', temperatureC: 0, summary: '' });
        } catch (error) {
            console.error('Error posting forecast:', error);
        }
    };

    const deleteForecast = async (id?: number) => {
        if (!id) return;
        try {
            const response = await fetch(`/api/weatherforecast/${id}`, {
                method: 'DELETE',
            });
            if (!response.ok) throw new Error('Failed to delete');
            await populateWeatherData();
        } catch (error) {
            console.error('Error deleting forecast:', error);
        }
    };

    return (
        <div>
            <h1 id="tableLabel">Weather forecast</h1>
            <p>This component demonstrates fetching, adding, and deleting data from the server.</p>

            {/* Add New Forecast Form */}
            <div>
                <h2>Add New Forecast</h2>
                <input type="date" name="date" value={newForecast.date} onChange={handleInputChange} />
                <input type="number" name="temperatureC" value={newForecast.temperatureC} onChange={handleInputChange} placeholder="Temp C" />
                <input type="text" name="summary" value={newForecast.summary} onChange={handleInputChange} placeholder="Summary" />
                <button onClick={addForecast}>Add</button>
            </div>

            {/* Forecast Table */}
            <table className="table table-striped" aria-labelledby="tableLabel">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {forecasts.map(forecast => (
                        <tr key={forecast.id}>
                            <td>{forecast.date}</td>
                            <td>{forecast.temperatureC}</td>
                            <td>{forecast.temperatureF}</td>
                            <td>{forecast.summary}</td>
                            <td>
                                <button onClick={() => deleteForecast(forecast.id)}>Delete</button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}

export default App;
