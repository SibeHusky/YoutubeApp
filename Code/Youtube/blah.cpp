class Blah
{
	private:
	const int x;
	public:
	Blah(int y) : x(y) { }
	const &int GetX() { return &x; }
}
int main()
{
	Blah test(5);
	*(test->GetX()) = 6;
}