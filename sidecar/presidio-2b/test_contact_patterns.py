"""Stdlib checks for lab contact patterns. No Presidio or spaCy required."""
import unittest

from contact_patterns import find_contacts


class ContactPatternTests(unittest.TestCase):
    def test_email_pt_phone_mz_phone_and_nif(self) -> None:
        text = (
            "Email maria.lab@example.co.mz "
            "tel +258 84 123 4567 e +351 912 345 678 "
            "NIF 123456789"
        )
        hits = find_contacts(text)
        spanned = [text[h["start"] : h["end"]] for h in hits]

        self.assertIn("maria.lab@example.co.mz", spanned)
        self.assertIn("+258 84 123 4567", spanned)
        self.assertIn("+351 912 345 678", spanned)
        self.assertIn("123456789", spanned)
        self.assertTrue(any(h["entity_type"] == "EMAIL_ADDRESS" for h in hits))
        self.assertTrue(any(h["entity_type"] == "PHONE_NUMBER" for h in hits))
        self.assertTrue(any(h["entity_type"] == "NIF" for h in hits))

    def test_compact_mz_mobile(self) -> None:
        text = "Telemovel +258841234567 no CV."
        spanned = [text[h["start"] : h["end"]] for h in find_contacts(text)]
        self.assertIn("+258841234567", spanned)


if __name__ == "__main__":
    unittest.main()
