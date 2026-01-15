namespace ScientistCardGame.Services
{
    public static class CharacterBiographies
    {
        public static string GetBiography(string characterName)
        {
            return characterName switch
            {
                // LEGENDARY TIER (10 characters)
                "Albert Einstein" =>
                    "🌟 ALBERT EINSTEIN (1879-1955)\n\n" +
                    "German-born theoretical physicist who revolutionized our understanding of space, time, and energy. " +
                    "Developed the theory of relativity (E=mc²), one of the two pillars of modern physics. " +
                    "Received the Nobel Prize in Physics in 1921 for his discovery of the photoelectric effect. " +
                    "His work laid the foundation for nuclear energy and quantum mechanics, changing science forever.",

                "Prophet Muhammad ﷺ" =>
                    "🌟 PROPHET MUHAMMAD ﷺ (570-632 CE)\n\n" +
                    "The final Prophet in Islam, born in Mecca. Received divine revelations forming the Quran. " +
                    "Emphasized knowledge, justice, compassion, and equality. Said 'Seeking knowledge is obligatory upon every Muslim.' " +
                    "Established principles of human rights and social justice. His life exemplifies mercy, wisdom, and dedication to truth, " +
                    "influencing over 1.8 billion Muslims today.",

                "Isaac Newton" =>
                    "🌟 SIR ISAAC NEWTON (1643-1727)\n\n" +
                    "English mathematician, physicist, and astronomer. Formulated the laws of motion and universal gravitation. " +
                    "Invented calculus independently. His 'Principia Mathematica' (1687) is one of the most important scientific books ever written. " +
                    "Discovered that white light is composed of colors. His laws of motion remain fundamental to physics today.",

                "Leonardo da Vinci" =>
                    "🌟 LEONARDO DA VINCI (1452-1519)\n\n" +
                    "Italian Renaissance polymath: painter, sculptor, architect, scientist, and engineer. " +
                    "Created masterpieces like the Mona Lisa and The Last Supper. Designed flying machines and armored vehicles centuries ahead of his time. " +
                    "Studied anatomy with unprecedented detail. Embodied the Renaissance ideal of excelling in both arts and sciences.",

                "Marie Curie" =>
                    "🌟 MARIE CURIE (1867-1934)\n\n" +
                    "Polish-French physicist and chemist, pioneer in radioactivity research. " +
                    "First woman to win a Nobel Prize, first person to win in two sciences (Physics 1903, Chemistry 1911). " +
                    "Discovered radium and polonium. Her research led to X-rays in surgery. " +
                    "Founded the Curie Institutes, major cancer research centers. Symbol of perseverance and excellence.",

                "Aristotle" =>
                    "🌟 ARISTOTLE (384-322 BCE)\n\n" +
                    "Ancient Greek philosopher, student of Plato and teacher of Alexander the Great. " +
                    "Made foundational contributions to logic, metaphysics, ethics, biology, and political theory. " +
                    "Established the Lyceum school. His works on logic formed the basis of Western philosophy for centuries. " +
                    "Studied and classified hundreds of animal species, shaping Western intellectual tradition.",

                "Socrates" =>
                    "🌟 SOCRATES (470-399 BCE)\n\n" +
                    "Ancient Greek philosopher, founder of Western philosophy. " +
                    "Developed the Socratic method of questioning to stimulate critical thinking. " +
                    "Never wrote his teachings; known through Plato's dialogues. Emphasized ethics and self-knowledge ('Know thyself'). " +
                    "Chose death over compromising his principles, becoming a martyr for truth and intellectual freedom.",

                "Charles Darwin" =>
                    "🌟 CHARLES DARWIN (1809-1882)\n\n" +
                    "English naturalist and biologist, father of evolutionary theory. " +
                    "Proposed that all species descend from common ancestors through natural selection. " +
                    "'On the Origin of Species' (1859) revolutionized biology. Research during HMS Beagle voyage shaped his theory. " +
                    "Provided the unifying framework for biological sciences, changing our understanding of life.",

                "Galileo Galilei" =>
                    "🌟 GALILEO GALILEI (1564-1642)\n\n" +
                    "Italian astronomer and physicist, father of modern observational astronomy. " +
                    "Improved the telescope and discovered Jupiter's moons, Saturn's rings, and sunspots. " +
                    "Championed heliocentrism (Earth revolves around Sun). Made fundamental contributions to physics. " +
                    "His scientific method based on observation shaped modern science. Faced persecution but stood by truth.",

                "Nikola Tesla" =>
                    "🌟 NIKOLA TESLA (1856-1943)\n\n" +
                    "Serbian-American inventor, pioneer of alternating current (AC) electricity. " +
                    "Developed the AC induction motor and polyphase system powering our world. " +
                    "Invented the Tesla coil and contributed to radio and wireless communication. " +
                    "Held over 300 patents and envisioned wireless power. A visionary decades ahead of his time.",

                // MASTER TIER (30 characters) - Scientists & Philosophers
                "Niels Bohr" =>
                    "🌟 NIELS BOHR (1885-1962)\n\n" +
                    "Danish physicist who made foundational contributions to quantum mechanics and atomic structure. " +
                    "Proposed the Bohr model of the atom. Won Nobel Prize in Physics 1922. " +
                    "Founded the Institute for Theoretical Physics in Copenhagen. His complementarity principle " +
                    "resolved paradoxes in quantum mechanics. Mentored a generation of quantum physicists.",

                "René Descartes" =>
                    "🌟 RENÉ DESCARTES (1596-1650)\n\n" +
                    "French philosopher, mathematician, and scientist. Father of modern philosophy. " +
                    "Famous for 'I think, therefore I am' (Cogito, ergo sum). Developed Cartesian coordinate system " +
                    "linking geometry and algebra. Made contributions to optics and the scientific method. " +
                    "His rationalist philosophy emphasized reason and doubt as paths to knowledge.",

                "Max Planck" =>
                    "🌟 MAX PLANCK (1858-1947)\n\n" +
                    "German theoretical physicist who originated quantum theory. " +
                    "Discovered energy quanta, revolutionizing our understanding of atomic processes. " +
                    "Won Nobel Prize in Physics 1918. Planck's constant is fundamental to quantum mechanics. " +
                    "His work laid the foundation for modern physics despite initially resisting quantum interpretation.",

                "Stephen Hawking" =>
                    "🌟 STEPHEN HAWKING (1942-2018)\n\n" +
                    "English theoretical physicist and cosmologist. Made groundbreaking work on black holes and cosmology. " +
                    "Discovered that black holes emit radiation (Hawking radiation). Author of 'A Brief History of Time'. " +
                    "Despite ALS diagnosis, continued brilliant scientific work for 50+ years. " +
                    "Made complex physics accessible to millions, inspiring worldwide fascination with the universe.",

                "Ada Lovelace" =>
                    "🌟 ADA LOVELACE (1815-1852)\n\n" +
                    "English mathematician, world's first computer programmer. " +
                    "Wrote the first algorithm for Charles Babbage's Analytical Engine. " +
                    "Envisioned computers beyond mere calculation, predicting their creative potential. " +
                    "Her notes contained the first published algorithm, making her the first programmer. " +
                    "Pioneered concepts that wouldn't be realized for a century.",

                "Thomas Edison" =>
                    "🌟 THOMAS EDISON (1847-1931)\n\n" +
                    "American inventor and businessman. Invented the phonograph and practical electric light bulb. " +
                    "Held 1,093 US patents. Established the first industrial research laboratory. " +
                    "Developed the motion picture camera and electric power distribution. " +
                    "His inventions transformed daily life and launched the electrical age. Famous for persistence and innovation.",

                "Richard Feynman" =>
                    "🌟 RICHARD FEYNMAN (1918-1988)\n\n" +
                    "American theoretical physicist, pioneer in quantum electrodynamics. " +
                    "Won Nobel Prize in Physics 1965. Developed Feynman diagrams revolutionizing particle physics. " +
                    "Contributed to the Manhattan Project. Renowned for making physics accessible and exciting. " +
                    "His lectures and books inspired generations. Combined brilliance with humor and curiosity.",

                "Carl Sagan" =>
                    "🌟 CARL SAGAN (1934-1996)\n\n" +
                    "American astronomer, cosmologist, and science communicator. " +
                    "Researched extraterrestrial intelligence and planetary science. Helped design Pioneer and Voyager probes. " +
                    "Created 'Cosmos' TV series, bringing science to millions. Popularized the phrase 'billions and billions'. " +
                    "Advocated for scientific thinking and skepticism. Made astronomy accessible and inspiring.",

                "Immanuel Kant" =>
                    "🌟 IMMANUEL KANT (1724-1804)\n\n" +
                    "German philosopher, central figure in modern philosophy. " +
                    "Developed critical philosophy examining reason's limits. 'Critique of Pure Reason' revolutionized epistemology. " +
                    "Proposed categorical imperative in ethics. Bridged rationalism and empiricism. " +
                    "His work on autonomy, freedom, and morality remains profoundly influential. Shaped modern thought.",

                "Friedrich Nietzsche" =>
                    "🌟 FRIEDRICH NIETZSCHE (1844-1900)\n\n" +
                    "German philosopher and cultural critic. Challenged Christian morality and traditional values. " +
                    "Proclaimed 'God is dead' reflecting secularization. Developed concepts of Übermensch and eternal recurrence. " +
                    "Emphasized will to power and individual creativity. His provocative ideas influenced existentialism. " +
                    "Misunderstood and controversial, yet profoundly impactful on modern thought.",

                "Buddha" =>
                    "🌟 BUDDHA (SIDDHARTHA GAUTAMA) (c. 563-483 BCE)\n\n" +
                    "Founder of Buddhism, born as prince in Nepal. Renounced luxury seeking enlightenment. " +
                    "Attained enlightenment under Bodhi tree, understanding suffering's end. Taught the Middle Way. " +
                    "Established Four Noble Truths and Eightfold Path. Emphasized compassion, mindfulness, and wisdom. " +
                    "His teachings spread across Asia, influencing hundreds of millions. Offered path to inner peace.",

                "Confucius" =>
                    "🌟 CONFUCIUS (551-479 BCE)\n\n" +
                    "Chinese philosopher and teacher whose teachings shaped East Asian culture. " +
                    "Emphasized virtue, morality, and proper social relationships. Taught jen (benevolence) and li (ritual propriety). " +
                    "His Analects compiled his wisdom. Stressed education, family respect, and ethical governance. " +
                    "Confucianism became state ideology in China. Influenced billions across millennia.",

                "Laozi" =>
                    "🌟 LAOZI (6th century BCE)\n\n" +
                    "Ancient Chinese philosopher, founder of Taoism. Authored 'Tao Te Ching', fundamental text of Eastern philosophy. " +
                    "Taught following the Tao (the Way), living in harmony with nature. " +
                    "Emphasized wu wei (effortless action), simplicity, and spontaneity. " +
                    "His mystical and paradoxical wisdom influenced Chinese culture profoundly. Offered alternative to Confucianism.",

                "Plato" =>
                    "🌟 PLATO (428-348 BCE)\n\n" +
                    "Ancient Greek philosopher, student of Socrates, teacher of Aristotle. " +
                    "Founded the Academy in Athens, first institution of higher learning. " +
                    "Developed Theory of Forms: abstract perfect forms underlie reality. " +
                    "Wrote influential dialogues including 'Republic' on justice and ideal state. " +
                    "His philosophy shaped Western thought for 2,400 years.",

                "John Locke" =>
                    "🌟 JOHN LOCKE (1632-1704)\n\n" +
                    "English philosopher and physician, father of liberalism. " +
                    "Proposed tabula rasa: mind as blank slate shaped by experience. " +
                    "Advocated natural rights to life, liberty, and property. Influenced democratic governance. " +
                    "His social contract theory shaped American and French revolutions. " +
                    "Pioneer of empiricism and political philosophy.",

                "David Hume" =>
                    "🌟 DAVID HUME (1711-1776)\n\n" +
                    "Scottish Enlightenment philosopher and historian. " +
                    "Developed empiricism arguing knowledge comes from sense experience. " +
                    "Skeptical of causation and religious belief. Influenced modern philosophy profoundly. " +
                    "His problem of induction challenged scientific reasoning. " +
                    "Wrote 'A Treatise of Human Nature'. Shaped epistemology and ethics.",

                "Jean-Jacques Rousseau" =>
                    "🌟 JEAN-JACQUES ROUSSEAU (1712-1778)\n\n" +
                    "Genevan philosopher who influenced Enlightenment and Romanticism. " +
                    "Wrote 'Social Contract' on legitimate political authority. " +
                    "Argued civilization corrupts natural human goodness. Influenced French Revolution. " +
                    "Pioneer of educational philosophy in 'Emile'. " +
                    "His ideas on freedom, equality, and democracy shaped modern politics.",

                "Sigmund Freud" =>
                    "🌟 SIGMUND FREUD (1856-1939)\n\n" +
                    "Austrian neurologist, founder of psychoanalysis. " +
                    "Developed theories of unconscious mind, id/ego/superego, and dream interpretation. " +
                    "Explored sexuality's role in psychology. Created talk therapy method. " +
                    "Controversial yet transformative in understanding human mind. " +
                    "His theories influenced psychology, psychiatry, and culture profoundly.",

                "Carl Jung" =>
                    "🌟 CARL JUNG (1875-1961)\n\n" +
                    "Swiss psychiatrist and psychoanalyst, founded analytical psychology. " +
                    "Developed concepts of collective unconscious and archetypes. " +
                    "Explored personality types: introversion and extraversion. " +
                    "Studied symbols, dreams, and mythology's psychological significance. " +
                    "His ideas influenced psychology, anthropology, and spirituality. Offered alternative to Freud.",

                "Mahatma Gandhi" =>
                    "🌟 MAHATMA GANDHI (1869-1948)\n\n" +
                    "Indian lawyer and political activist. Led India's independence movement through nonviolent civil disobedience. " +
                    "Philosophy of Satyagraha (truth force) inspired civil rights movements worldwide. " +
                    "Advocated simple living, vegetarianism, and religious tolerance. " +
                    "Influenced Martin Luther King Jr. and Nelson Mandela. " +
                    "Demonstrated power of peaceful resistance. 'Be the change you wish to see.'",

                "Martin Luther King Jr." =>
                    "🌟 MARTIN LUTHER KING JR. (1929-1968)\n\n" +
                    "American Baptist minister and civil rights leader. " +
                    "Led Civil Rights Movement through nonviolent resistance. " +
                    "Delivered iconic 'I Have a Dream' speech. Won Nobel Peace Prize 1964. " +
                    "Fought against racial segregation and discrimination. " +
                    "Assassinated at 39, but his legacy transformed America. Symbol of justice and equality.",

                "Nelson Mandela" =>
                    "🌟 NELSON MANDELA (1918-2013)\n\n" +
                    "South African anti-apartheid revolutionary and president. " +
                    "Imprisoned 27 years for fighting racial oppression. " +
                    "Led peaceful transition from apartheid to democracy. " +
                    "South Africa's first black president (1994-1999). Promoted reconciliation over revenge. " +
                    "Won Nobel Peace Prize. Global symbol of resistance to injustice and forgiveness.",

                "Voltaire" =>
                    "🌟 VOLTAIRE (1694-1778)\n\n" +
                    "French Enlightenment writer and philosopher. " +
                    "Advocate of freedom of speech, religion, and separation of church and state. " +
                    "Famous for wit and criticism of intolerance. Wrote 'Candide' satirizing optimism. " +
                    "Influenced French Revolution. Defended civil liberties courageously. " +
                    "His ideas shaped modern democracy and human rights.",

                "Baruch Spinoza" =>
                    "🌟 BARUCH SPINOZA (1632-1677)\n\n" +
                    "Dutch philosopher of Portuguese-Jewish origin. " +
                    "Developed rationalist philosophy identifying God with Nature. " +
                    "Advocated freedom of thought in democratic society. " +
                    "His 'Ethics' presented geometric method in philosophy. " +
                    "Excommunicated for controversial views but profoundly influenced Enlightenment. " +
                    "Pioneer of biblical criticism and modern secularism.",

                "Søren Kierkegaard" =>
                    "🌟 SØREN KIERKEGAARD (1813-1855)\n\n" +
                    "Danish philosopher and theologian, father of existentialism. " +
                    "Emphasized individual existence, choice, and subjective truth. " +
                    "Explored anxiety, despair, and leap of faith. Criticized organized religion. " +
                    "Influenced existentialism profoundly. Stressed authentic living and personal responsibility. " +
                    "His psychological insights remain deeply relevant.",

                "Arthur Schopenhauer" =>
                    "🌟 ARTHUR SCHOPENHAUER (1788-1860)\n\n" +
                    "German philosopher known for pessimistic philosophy. " +
                    "Argued will is the fundamental reality causing suffering. " +
                    "Influenced by Eastern philosophy, particularly Buddhism. " +
                    "Believed art and compassion offer escape from suffering. " +
                    "Influenced Nietzsche, Freud, and existentialists. " +
                    "His dark insights into human nature remain influential.",

                "Bertrand Russell" =>
                    "🌟 BERTRAND RUSSELL (1872-1970)\n\n" +
                    "British philosopher, logician, and social critic. " +
                    "Co-authored 'Principia Mathematica' revolutionizing logic. " +
                    "Advocated rationalism, pacifism, and nuclear disarmament. " +
                    "Won Nobel Prize in Literature 1950. Contributed to analytic philosophy. " +
                    "His clear thinking and activism influenced 20th century profoundly.",

                "Ludwig Wittgenstein" =>
                    "🌟 LUDWIG WITTGENSTEIN (1889-1951)\n\n" +
                    "Austrian-British philosopher who revolutionized philosophy of language. " +
                    "'Tractatus Logico-Philosophicus' explored language's limits. " +
                    "Later work 'Philosophical Investigations' examined language games. " +
                    "Argued many philosophical problems arise from language misuse. " +
                    "One of the most influential 20th-century philosophers.",

                "Albert Camus" =>
                    "🌟 ALBERT CAMUS (1913-1960)\n\n" +
                    "French-Algerian philosopher and novelist. " +
                    "Explored absurdism: life's inherent meaninglessness. " +
                    "Wrote 'The Stranger' and 'The Myth of Sisyphus'. " +
                    "Argued for revolt, freedom, and passion despite absurdity. " +
                    "Won Nobel Prize in Literature 1957. " +
                    "His philosophy emphasized living fully despite life's absurdity.",

                "Thomas Aquinas" =>
                    "🌟 THOMAS AQUINAS (1225-1274)\n\n" +
                    "Italian Dominican friar and theologian. " +
                    "Synthesized Aristotelian philosophy with Christian theology. " +
                    "'Summa Theologica' is masterpiece of medieval thought. " +
                    "Developed five proofs for God's existence. " +
                    "Influenced Catholic doctrine profoundly. Doctor of the Church. " +
                    "His natural law theory shaped ethics and jurisprudence.",

                // SCHOLAR TIER (60 characters) - Mix of scientists, thinkers, innovators
                "Archimedes" =>
                    "🌟 ARCHIMEDES (c. 287-212 BCE)\n\n" +
                    "Ancient Greek mathematician, physicist, and engineer. " +
                    "Discovered principles of leverage, buoyancy, and hydrostatics. " +
                    "Invented many machines and weapons. Famous for 'Eureka!' moment. " +
                    "His mathematical methods anticipated calculus. Greatest mathematician of antiquity.",

                "Pythagoras" =>
                    "🌟 PYTHAGORAS (c. 570-495 BCE)\n\n" +
                    "Ancient Greek philosopher and mathematician. " +
                    "Founded Pythagorean school studying mathematics, music, and philosophy. " +
                    "Famous for Pythagorean theorem in geometry. " +
                    "Believed numbers are the essence of reality. " +
                    "Influenced mathematics and mystical thought profoundly.",

                "Euclid" =>
                    "🌟 EUCLID (c. 300 BCE)\n\n" +
                    "Greek mathematician, father of geometry. " +
                    "'Elements' is one of the most influential mathematical works ever. " +
                    "Systematized geometric knowledge using axioms and proofs. " +
                    "His logical approach became the model for mathematics. " +
                    "Used as textbook for 2,000 years.",

                "Hippocrates" =>
                    "🌟 HIPPOCRATES (c. 460-370 BCE)\n\n" +
                    "Ancient Greek physician, father of medicine. " +
                    "Separated medicine from superstition, emphasizing observation. " +
                    "Hippocratic Oath guides medical ethics today. " +
                    "Established medicine as profession. " +
                    "His rational approach transformed healthcare.",

                "Louis Pasteur" =>
                    "🌟 LOUIS PASTEUR (1822-1895)\n\n" +
                    "French chemist and microbiologist. " +
                    "Developed pasteurization process and vaccines for rabies and anthrax. " +
                    "Proved germ theory of disease. " +
                    "Founded microbiology. Saved millions of lives. " +
                    "His work revolutionized medicine and public health.",

                "Michael Faraday" =>
                    "🌟 MICHAEL FARADAY (1791-1867)\n\n" +
                    "English scientist who contributed to electromagnetism and electrochemistry. " +
                    "Discovered electromagnetic induction, basis of electric generators. " +
                    "Self-taught, rose from poverty to scientific greatness. " +
                    "His experiments laid groundwork for electrical age.",

                "James Clerk Maxwell" =>
                    "🌟 JAMES CLERK MAXWELL (1831-1879)\n\n" +
                    "Scottish physicist who formulated classical electromagnetic theory. " +
                    "His equations unified electricity, magnetism, and light. " +
                    "Predicted radio waves. Contributed to kinetic theory. " +
                    "Einstein called his work 'the most profound change in physics.'",

                "Ernest Rutherford" =>
                    "🌟 ERNEST RUTHERFORD (1871-1937)\n\n" +
                    "New Zealand physicist, father of nuclear physics. " +
                    "Discovered atomic nucleus and proton. " +
                    "First to split the atom. Won Nobel Prize in Chemistry 1908. " +
                    "His work transformed understanding of atomic structure.",

                "Werner Heisenberg" =>
                    "🌟 WERNER HEISENBERG (1901-1976)\n\n" +
                    "German physicist, pioneer of quantum mechanics. " +
                    "Formulated uncertainty principle: cannot precisely know position and momentum simultaneously. " +
                    "Developed matrix mechanics. Won Nobel Prize 1932. " +
                    "His insights fundamentally changed physics.",

                "Erwin Schrödinger" =>
                    "🌟 ERWIN SCHRÖDINGER (1887-1961)\n\n" +
                    "Austrian physicist who developed wave equation in quantum mechanics. " +
                    "Famous for Schrödinger's cat thought experiment. " +
                    "Won Nobel Prize 1933. Contributed to quantum theory profoundly. " +
                    "Explored connections between physics and biology.",

                "Paul Dirac" =>
                    "🌟 PAUL DIRAC (1902-1984)\n\n" +
                    "British physicist who made fundamental contributions to quantum mechanics. " +
                    "Predicted antimatter existence. Dirac equation describes relativistic electrons. " +
                    "Won Nobel Prize 1933. Known for mathematical beauty in physics. " +
                    "One of the founders of quantum field theory.",

                "Enrico Fermi" =>
                    "🌟 ENRICO FERMI (1901-1954)\n\n" +
                    "Italian-American physicist, created first nuclear reactor. " +
                    "Contributed to quantum theory, nuclear physics, and particle physics. " +
                    "Led Manhattan Project team. Won Nobel Prize 1938. " +
                    "Fermi paradox questions absence of alien contact.",

                "J. Robert Oppenheimer" =>
                    "🌟 J. ROBERT OPPENHEIMER (1904-1967)\n\n" +
                    "American physicist, led Manhattan Project developing atomic bomb. " +
                    "Brilliant theoretical physicist. Quoted Bhagavad Gita: 'I am become death.' " +
                    "Later opposed nuclear weapons proliferation. " +
                    "Complex legacy of scientific achievement and moral struggle.",

                "Edwin Hubble" =>
                    "🌟 EDWIN HUBBLE (1889-1953)\n\n" +
                    "American astronomer who revolutionized cosmology. " +
                    "Proved galaxies exist beyond Milky Way. " +
                    "Discovered universe is expanding (Hubble's Law). " +
                    "Hubble Space Telescope named after him. " +
                    "Changed our understanding of universe's scale and evolution.",

                "Copernicus" =>
                    "🌟 NICOLAUS COPERNICUS (1473-1543)\n\n" +
                    "Polish astronomer who proposed heliocentric model: Sun at center, Earth orbits. " +
                    "Challenged 1,400-year-old geocentric model. " +
                    "His book sparked Copernican Revolution. " +
                    "Transformed astronomy and human understanding of our place in cosmos.",

                "Johannes Kepler" =>
                    "🌟 JOHANNES KEPLER (1571-1630)\n\n" +
                    "German astronomer and mathematician. " +
                    "Discovered laws of planetary motion. " +
                    "Planets orbit in ellipses with Sun at focus. " +
                    "His work confirmed heliocentric model and enabled Newton's theory. " +
                    "Founded modern astronomy.",

                "Gregor Mendel" =>
                    "🌟 GREGOR MENDEL (1822-1884)\n\n" +
                    "Austrian monk, father of genetics. " +
                    "Discovered laws of inheritance through pea plant experiments. " +
                    "His work unrecognized until rediscovered decades later. " +
                    "Laid foundation for genetics. " +
                    "Proved traits pass from generation to generation predictably.",

                "Antoine Lavoisier" =>
                    "🌟 ANTOINE LAVOISIER (1743-1794)\n\n" +
                    "French chemist, father of modern chemistry. " +
                    "Discovered oxygen's role in combustion. " +
                    "Formulated law of conservation of mass. " +
                    "Named and defined chemical elements. Executed during French Revolution. " +
                    "Transformed chemistry from alchemy to science.",

                "Dmitri Mendeleev" =>
                    "🌟 DMITRI MENDELEEV (1834-1907)\n\n" +
                    "Russian chemist who created the periodic table. " +
                    "Arranged elements by atomic weight and properties. " +
                    "Predicted undiscovered elements' properties correctly. " +
                    "His table remains fundamental to chemistry. " +
                    "Element 101, Mendelevium, named after him.",

                "Alexander Fleming" =>
                    "🌟 ALEXANDER FLEMING (1881-1955)\n\n" +
                    "Scottish bacteriologist who discovered penicillin. " +
                    "Accidental discovery in 1928 revolutionized medicine. " +
                    "First antibiotic saved millions of lives. " +
                    "Won Nobel Prize 1945. " +
                    "His work ushered in the antibiotic era.",

                "Jonas Salk" =>
                    "🌟 JONAS SALK (1914-1995)\n\n" +
                    "American virologist who developed first polio vaccine. " +
                    "Vaccine ended polio epidemics devastating children. " +
                    "Refused to patent vaccine, making it affordable globally. " +
                    "His selflessness saved countless lives. " +
                    "Hero of public health.",

                "Francis Crick & James Watson" =>
                    "🌟 CRICK & WATSON (1916-2004 & 1928-)\n\n" +
                    "British and American biologists who discovered DNA's double helix structure. " +
                    "Their 1953 discovery revolutionized biology. " +
                    "Enabled understanding of genetic code and heredity. " +
                    "Won Nobel Prize 1962. Founded molecular biology. " +
                    "Their work transformed medicine and genetics.",

                "Alan Turing" =>
                    "🌟 ALAN TURING (1912-1954)\n\n" +
                    "British mathematician, father of computer science and AI. " +
                    "Broke Nazi Enigma code, helping win WWII. " +
                    "Developed Turing machine concept and Turing test. " +
                    "Laid foundations for modern computing. " +
                    "Tragically persecuted for homosexuality. Posthumously pardoned.",

                "Claude Shannon" =>
                    "🌟 CLAUDE SHANNON (1916-2001)\n\n" +
                    "American mathematician, father of information theory. " +
                    "Founded digital circuit design theory. " +
                    "Defined information quantitatively. " +
                    "Enabled digital communication and computing. " +
                    "His master's thesis called 'most important of the 20th century.'",

                "Tim Berners-Lee" =>
                    "🌟 TIM BERNERS-LEE (1955-)\n\n" +
                    "British computer scientist who invented the World Wide Web. " +
                    "Created HTTP, HTML, and first web browser (1989-1991). " +
                    "Made invention freely available without patent. " +
                    "Transformed global communication and information access. " +
                    "His gift to humanity changed the world.",

                "Grace Hopper" =>
                    "🌟 GRACE HOPPER (1906-1992)\n\n" +
                    "American computer scientist and naval officer. " +
                    "Pioneer in computer programming, developed first compiler. " +
                    "Created COBOL programming language. " +
                    "Popularized 'debugging' term (found actual moth in computer). " +
                    "Her innovations made programming accessible.",

                "Rosalind Franklin" =>
                    "🌟 ROSALIND FRANKLIN (1920-1958)\n\n" +
                    "British chemist whose X-ray crystallography was crucial to discovering DNA structure. " +
                    "Her Photo 51 provided key evidence for double helix. " +
                    "Contributions underrecognized during lifetime. " +
                    "Also made important contributions to virus research. " +
                    "Died tragically young from cancer.",

                "Rachel Carson" =>
                    "🌟 RACHEL CARSON (1907-1964)\n\n" +
                    "American marine biologist and conservationist. " +
                    "'Silent Spring' (1962) exposed pesticide dangers, launching environmental movement. " +
                    "Faced industry attacks but persevered. " +
                    "Led to DDT ban and EPA creation. " +
                    "Her courage sparked global environmental awareness.",

                "Jane Goodall" =>
                    "🌟 JANE GOODALL (1934-)\n\n" +
                    "British primatologist and anthropologist. " +
                    "Revolutionized chimpanzee study through decades of observation. " +
                    "Discovered chimps use tools and have complex emotions. " +
                    "Challenged assumptions separating humans from animals. " +
                    "Tireless conservationist and humanitarian. UN Messenger of Peace.",

                "Carl Linnaeus" =>
                    "🌟 CARL LINNAEUS (1707-1778)\n\n" +
                    "Swedish botanist and zoologist, father of taxonomy. " +
                    "Created binomial nomenclature for naming species. " +
                    "Classified thousands of organisms systematically. " +
                    "His system remains fundamental to biology. " +
                    "Organized the natural world comprehensibly.",

                "Linus Pauling" =>
                    "🌟 LINUS PAULING (1901-1994)\n\n" +
                    "American chemist and peace activist. " +
                    "Only person to win two unshared Nobel Prizes (Chemistry 1954, Peace 1962). " +
                    "Pioneered quantum chemistry and molecular biology. " +
                    "Advocated for nuclear disarmament and vitamin C benefits. " +
                    "Brilliant scientist and humanitarian.",

                "Emmy Noether" =>
                    "🌟 EMMY NOETHER (1882-1935)\n\n" +
                    "German mathematician who revolutionized abstract algebra. " +
                    "Noether's theorem connects symmetries with conservation laws in physics. " +
                    "Einstein called her 'most significant creative mathematical genius.' " +
                    "Overcame gender barriers in mathematics. " +
                    "Her work is fundamental to modern physics.",

                "Srinivasa Ramanujan" =>
                    "🌟 SRINIVASA RAMANUJAN (1887-1920)\n\n" +
                    "Indian mathematician of extraordinary genius. " +
                    "Self-taught, made groundbreaking contributions to number theory. " +
                    "His intuitive insights continue to yield mathematical discoveries. " +
                    "Collaborated with Hardy at Cambridge. " +
                    "Died tragically young, leaving notebooks full of theorems.",

                "Marie Skłodowska-Curie" =>
                    "🌟 PIERRE CURIE (1859-1906)\n\n" +
                    "French physicist who discovered piezoelectricity. " +
                    "Collaborated with wife Marie in radioactivity research. " +
                    "Shared 1903 Nobel Prize in Physics. " +
                    "His magnetic insights (Curie point) advanced physics. " +
                    "Died in accident at 46, but legacy endures.",

                "Benjamin Franklin" =>
                    "🌟 BENJAMIN FRANKLIN (1706-1790)\n\n" +
                    "American polymath: scientist, inventor, writer, statesman. " +
                    "Proved lightning is electricity with kite experiment. " +
                    "Invented bifocals, lightning rod, Franklin stove. " +
                    "Founding Father of United States. " +
                    "Embodied Enlightenment ideals of reason and progress.",

                "Alexander Graham Bell" =>
                    "🌟 ALEXANDER GRAHAM BELL (1847-1922)\n\n" +
                    "Scottish-American inventor and scientist. " +
                    "Invented the telephone, revolutionizing communication. " +
                    "Also worked on aviation, hydrofoils, and helping the deaf. " +
                    "Founded Bell Telephone Company. " +
                    "His invention connected the world.",

                "Guglielmo Marconi" =>
                    "🌟 GUGLIELMO MARCONI (1874-1937)\n\n" +
                    "Italian inventor, pioneer of long-distance radio transmission. " +
                    "Sent first transatlantic radio signal (1901). " +
                    "Won Nobel Prize in Physics 1909. " +
                    "Founded wireless telegraphy. " +
                    "His work enabled modern telecommunications.",

                "Wright Brothers" =>
                    "🌟 WRIGHT BROTHERS (Orville 1871-1948, Wilbur 1867-1912)\n\n" +
                    "American inventors who achieved first powered, controlled flight (1903). " +
                    "Built first practical airplane. " +
                    "Self-taught engineers combined scientific method with innovation. " +
                    "Launched aviation age. " +
                    "Their achievement fulfilled humanity's ancient dream of flight.",

                "Louis de Broglie" =>
                    "🌟 LOUIS DE BROGLIE (1892-1987)\n\n" +
                    "French physicist who proposed wave-particle duality. " +
                    "Suggested particles have wave properties. " +
                    "His PhD thesis revolutionized quantum mechanics. " +
                    "Won Nobel Prize 1929. " +
                    "Unified wave and particle concepts.",

                "Max Born" =>
                    "🌟 MAX BORN (1882-1970)\n\n" +
                    "German physicist who made fundamental contributions to quantum mechanics. " +
                    "Gave statistical interpretation of wave function. " +
                    "Won Nobel Prize 1954. " +
                    "Mentored many quantum physicists. " +
                    "His work was crucial to quantum theory development.",

                "Wolfgang Pauli" =>
                    "🌟 WOLFGANG PAULI (1900-1958)\n\n" +
                    "Austrian physicist who formulated exclusion principle. " +
                    "No two electrons can have identical quantum states. " +
                    "This explains atomic structure and periodic table. " +
                    "Won Nobel Prize 1945. " +
                    "Known for high standards and critical thinking.",

                "Niels Henrik Abel" =>
                    "🌟 NIELS HENRIK ABEL (1802-1829)\n\n" +
                    "Norwegian mathematician who made pioneering contributions to algebra. " +
                    "Proved impossibility of solving quintic equations by radicals. " +
                    "Developed elliptic functions theory. " +
                    "Died in poverty at 26, recognition came posthumously. " +
                    "Abelian groups named after him.",

                "Évariste Galois" =>
                    "🌟 ÉVARISTE GALOIS (1811-1832)\n\n" +
                    "French mathematician who founded group theory. " +
                    "Solved centuries-old problem about polynomial equations. " +
                    "Wrote groundbreaking ideas night before fatal duel at 20. " +
                    "His work transformed algebra. " +
                    "Tragic genius whose brilliance was posthumously recognized.",

                "Georg Cantor" =>
                    "🌟 GEORG CANTOR (1845-1918)\n\n" +
                    "German mathematician who created set theory. " +
                    "Proved infinite sets have different sizes. " +
                    "His transfinite numbers revolutionized mathematics. " +
                    "Faced opposition but persevered. " +
                    "Founded modern mathematics' conceptual framework.",

                "Kurt Gödel" =>
                    "🌟 KURT GÖDEL (1906-1978)\n\n" +
                    "Austrian-American logician and mathematician. " +
                    "Proved incompleteness theorems: mathematics cannot be both complete and consistent. " +
                    "Revolutionized logic and philosophy. " +
                    "Friend of Einstein. " +
                    "His work shows fundamental limits of mathematical systems.",

                "Hippasus" =>
                    "🌟 HIPPASUS (5th century BCE)\n\n" +
                    "Greek mathematician who allegedly discovered irrational numbers. " +
                    "Proved square root of 2 cannot be expressed as fraction. " +
                    "According to legend, drowned for revealing this mathematical secret. " +
                    "His discovery challenged Pythagorean worldview. " +
                    "Martyr for mathematical truth.",

                "Omar Khayyam" =>
                    "🌟 OMAR KHAYYAM (1048-1131)\n\n" +
                    "Persian mathematician, astronomer, and poet. " +
                    "Made advances in algebra and geometry. " +
                    "Reformed calendar with remarkable accuracy. " +
                    "Famous for 'Rubaiyat' poetry celebrating life. " +
                    "Combined scientific brilliance with philosophical wisdom.",

                "Al-Khwarizmi" =>
                    "🌟 AL-KHWARIZMI (780-850)\n\n" +
                    "Persian mathematician, father of algebra. " +
                    "His book 'Al-Jabr' gave algebra its name. " +
                    "Introduced Hindu-Arabic numerals to West. " +
                    "'Algorithm' derived from his name. " +
                    "His work transformed mathematics and enabled scientific revolution.",

                "Avicenna (Ibn Sina)" =>
                    "🌟 AVICENNA (Ibn Sina) (980-1037)\n\n" +
                    "Persian polymath: physician, philosopher, astronomer. " +
                    "'The Canon of Medicine' used for 600 years. " +
                    "Synthesized Greek and Islamic philosophy. " +
                    "Made contributions to logic, metaphysics, and science. " +
                    "One of history's greatest thinkers.",

                "Averroes (Ibn Rushd)" =>
                    "🌟 AVERROES (Ibn Rushd) (1126-1198)\n\n" +
                    "Andalusian philosopher who defended rationalism. " +
                    "Wrote influential commentaries on Aristotle. " +
                    "Argued reason and faith are compatible. " +
                    "Influenced European medieval philosophy. " +
                    "His works preserved Greek philosophy for Renaissance.",

                "Al-Razi" =>
                    "🌟 AL-RAZI (Rhazes) (854-925)\n\n" +
                    "Persian physician and philosopher. " +
                    "Distinguished smallpox from measles. " +
                    "Pioneered use of alcohol as antiseptic. " +
                    "Wrote comprehensive medical encyclopedia. " +
                    "His experimental approach advanced medicine.",

                "Hypatia" =>
                    "🌟 HYPATIA (c. 350-415)\n\n" +
                    "Greek mathematician and philosopher in Alexandria. " +
                    "Taught mathematics, astronomy, and philosophy. " +
                    "First well-documented female mathematician. " +
                    "Edited mathematical texts of Euclid and others. " +
                    "Murdered by mob, symbol of learning destroyed by fanaticism.",

                "Zhang Heng" =>
                    "🌟 ZHANG HENG (78-139)\n\n" +
                    "Chinese astronomer, mathematician, and inventor. " +
                    "Created first seismoscope to detect earthquakes. " +
                    "Calculated pi accurately. Built armillary sphere. " +
                    "Mapped stars and documented lunar eclipses. " +
                    "Polymath who advanced Chinese science significantly.",

                "Ibn al-Haytham (Alhazen)" =>
                    "🌟 IBN AL-HAYTHAM (Alhazen) (965-1040)\n\n" +
                    "Arab mathematician and physicist, father of optics. " +
                    "Explained vision through refraction, not emission. " +
                    "Developed scientific method emphasizing experimentation. " +
                    "His 'Book of Optics' influenced Western science. " +
                    "Made contributions to mathematics and astronomy.",

                "Ada Yonath" =>
                    "🌟 ADA YONATH (1939-)\n\n" +
                    "Israeli crystallographer who mapped ribosome structure. " +
                    "Won Nobel Prize in Chemistry 2009. " +
                    "Her work enables antibiotic development. " +
                    "Overcame childhood poverty to achieve scientific greatness. " +
                    "Pioneer in structural biology.",

                "Tu Youyou" =>
                    "🌟 TU YOUYOU (1930-)\n\n" +
                    "Chinese pharmaceutical chemist. " +
                    "Discovered artemisinin, antimalarial drug saving millions. " +
                    "Won Nobel Prize 2015. " +
                    "Combined traditional Chinese medicine with modern science. " +
                    "Her work demonstrates value of traditional knowledge.",

                "Vera Rubin" =>
                    "🌟 VERA RUBIN (1928-2016)\n\n" +
                    "American astronomer who discovered evidence for dark matter. " +
                    "Measured galaxy rotation rates revealing invisible mass. " +
                    "Her work revolutionized cosmology. " +
                    "Advocated for women in science. " +
                    "Proved universe is stranger than we imagined.",

                "Chien-Shiung Wu" =>
                    "🌟 CHIEN-SHIUNG WU (1912-1997)\n\n" +
                    "Chinese-American physicist who disproved parity conservation. " +
                    "Her experiment confirmed theoretical prediction (colleagues won Nobel). " +
                    "Contributed to Manhattan Project. " +
                    "Known as 'First Lady of Physics' and 'Chinese Marie Curie'. " +
                    "Her experimental skill was legendary.",

                "Dante Alighieri" =>
                    "🌟 DANTE ALIGHIERI (1265-1321)\n\n" +
                    "Italian poet and philosopher. " +
                    "Wrote 'Divine Comedy', masterpiece of world literature. " +
                    "Explored theology, philosophy, and ethics through allegory. " +
                    "Shaped Italian language. " +
                    "His vision of Hell, Purgatory, and Paradise remains iconic. " +
                    "Combined poetry with deep philosophical insight.",

                // Default for any character not listed
                _ => $"📚 {characterName}\n\n" +
                     "A remarkable figure in the history of science, philosophy, and human knowledge. " +
                     "Their contributions have shaped our understanding of the world and continue to inspire new generations. " +
                     "Through dedication, curiosity, and brilliance, they expanded the boundaries of human achievement."
            };
        }
    }
}